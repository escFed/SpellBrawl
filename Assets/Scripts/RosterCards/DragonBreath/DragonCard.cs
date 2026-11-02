
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

public class DragonCard : MonoBehaviour, ICardable
{
    [SerializeField] private string cardName = "Dragon Breath";
    [SerializeField][TextArea] private string cardDescription = "Invokes a continuous fire breath attack";
    [SerializeField] private CardType type = CardType.Offensive;
    [SerializeField] private Sprite cardVisual = null;
    [SerializeField] private string damageableOrNot = "Damageable";

    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => type;
    public Sprite CardVisual => cardVisual;
    public string DamageableOrNot => damageableOrNot;


    [Header("Visual")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private UI.Image cardUI;


    [SerializeField] private GameObject dragonPrefab;




    public void SetUI(UI.Image uiImage)
    {
        cardUI = uiImage;

        if (cardUI != null)
        {
            cardUI.sprite = cardSprite;

            cardUI.enabled = true;
        }
    }

    public bool CanBeUsed(CharacterCoordinator user) => true;
    public void ExecuteCard(CharacterCoordinator character)
    {
        // Obtener todos los CharacterCoordinator en la escena
        CharacterCoordinator[] allPlayers = GameObject.FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);

        // Elegir el primer jugador que no sea el que usa la carta
        CharacterCoordinator target = null;
        foreach (CharacterCoordinator p in allPlayers)
        {
            if (p.gameObject != character.gameObject)
            {
                target = p;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogError("Dragon Breath: No se encontró enemigo!");
            return;
        }

        Transform spawnPoint = character.Grab != null && character.Grab.throwPoint != null
            ? character.Grab.throwPoint
            : character.transform;
        Vector3 spawnPosition = Vector3.Lerp(spawnPoint.position, target.transform.position, 0.5f);

        GameObject breath = Instantiate(dragonPrefab, spawnPosition, Quaternion.identity);
        GetComponent<MatchCardUse>()?.CopyTo(breath);
        breath.transform.localScale = Vector3.one * 2f;

        if (!breath.TryGetComponent(out Collider2D hitBox))
        {
            Debug.LogError("Dragon Breath: El prefab no tiene una hitbox Collider2D.", breath);
            Destroy(breath);
            return;
        }

        if (!breath.TryGetComponent(out DragonBreathLogic script))
        {
            Debug.LogError("Dragon Breath: El prefab no tiene DragonBreathLogic.", breath);
            Destroy(breath);
            return;
        }

        Physics2D.SyncTransforms();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(Physics2D.AllLayers);
        List<Collider2D> overlaps = new List<Collider2D>();
        hitBox.Overlap(filter, overlaps);

        bool detectedOpponent = false;
        foreach (Collider2D overlap in overlaps)
        {
            if (overlap != null && overlap.GetComponentInParent<CharacterCoordinator>() == target)
            {
                detectedOpponent = true;
                break;
            }
        }

        if (!detectedOpponent)
        {
            Destroy(breath);
            return;
        }

        script.SetTargetAndPrefab(target, breath, character.PlayerIndex);
        script.StartCoroutine(script.BreathCorroutine(target));

    }


}
