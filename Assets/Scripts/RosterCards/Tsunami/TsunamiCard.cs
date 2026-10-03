using UnityEngine;
using UnityEngine.UI;

public class TsunamiCard : MonoBehaviour, ICardable
{

    [Header("Card Info")]
    [SerializeField] private string cardName = "Tsunami";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sends a wave forward that pushes rivals while they remain inside it.";
    [SerializeField] private string damageableOrNot = "NON DAMAGEABLE";

    [Header("Card Settings")]
    [SerializeField] private CardType type = CardType.Utility;
    [SerializeField] private Sprite cardVisual;
    [SerializeField] private Image cardUI;

    [SerializeField] private GameObject tsunamiPrefab;
    [SerializeField, Min(0f)] private float spawnDistance = 3f;

    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => type;
    public Sprite CardVisual => cardVisual;
    public string DamageableOrNot => damageableOrNot;



    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
        if (cardUI != null)
        {
            cardUI.sprite = cardVisual;

            cardUI.enabled = true;
        }
    }

    public bool CanBeUsed(CharacterCoordinator user) => user != null && tsunamiPrefab != null;

    public void ExecuteCard(CharacterCoordinator character)
    {
        if (!CanBeUsed(character))
            return;

        CharacterHitBox hitBox = character.GetComponent<CharacterHitBox>();
        if (hitBox != null && Mathf.Abs(character.MoveInput.x) > 0.1f)
            hitBox.FaceDirection(character.MoveInput.x);

        float direction = hitBox != null
            ? (hitBox.IsFacingRight ? 1f : -1f)
            : Mathf.Sign(character.transform.localScale.x);
        if (direction == 0f)
            direction = 1f;

        Vector3 spawnPosition = character.transform.position + Vector3.right * (direction * spawnDistance);
        GameObject waveInstance = Instantiate(tsunamiPrefab, spawnPosition, Quaternion.identity);
        if (waveInstance.TryGetComponent(out TsunamiWave wave))
            wave.Init(character, direction);
        else
        {
            Debug.LogError("El prefab Tsunami no tiene TsunamiWave adjunto.", tsunamiPrefab);
            Destroy(waveInstance);
        }

        Destroy(gameObject);
    }


}
