using UnityEngine;
using UnityEngine.UI;

public class ThunderStrikeCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "ThunderStrike Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Unleashes a powerful thunder strike.";
    [SerializeField] private string damageOrNot = "8";

    [Header("Settings")]
    [SerializeField] private GameObject tsPrefab;
    [SerializeField] private int energyCost = 20;

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.OFFENSIVE;
    public string DamageableOrNot => damageOrNot;

    // Cambiado a Sprite para coincidir con ICardable.CardVisual
    public Sprite CardVisual => cardSprite;

    [Header("Visual")]
    [SerializeField] public Sprite cardSprite;
    [SerializeField] private Image cardUI;

    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;

        if (cardUI != null)
        {
            cardUI.sprite = cardSprite;

            cardUI.enabled = true;
        }
    }

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        PlayerController target = null;

        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            if (p.gameObject != character.gameObject)
            {
                target = p;
                break;
            }
        }

        if (target != null)
        {
            GameObject th = Instantiate(tsPrefab, target.transform.position, Quaternion.identity);

            if (th.TryGetComponent(out ThunderProjectile tProj))
            {
                tProj.Init(character.gameObject);
            }
        }

        Destroy(gameObject);
    }
}
