
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

        Transform spawnPoint = character.Grab.throwPoint != null ? character.Grab.throwPoint : character.transform;

        // 📍 CORREGIDO: Calcular posición entre el que lanza y el target
        Vector3 spawnPosition = Vector3.Lerp(spawnPoint.position, target.transform.position, 0.5f);

        GameObject breath = Instantiate(dragonPrefab, spawnPosition, Quaternion.identity);

        // 📏 CORREGIDO: Escalar el prefab para que sea visible
        breath.transform.localScale = Vector3.one * 2f; // Tamaño 2x más grande

        if (breath.TryGetComponent(out DragonBreathLogic script))
        {
            // ✅ Pasar el OBJETIVO (target) para causar daño
            script.SetTargetAndPrefab(target, breath);
            StartCoroutine(script.BreathCorroutine(target));
        }

    }


}
