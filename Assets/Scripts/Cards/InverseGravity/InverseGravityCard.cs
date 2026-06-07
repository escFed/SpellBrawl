using UnityEngine;
using UnityEngine.UI;

public class InverseGravityCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Sabotaje de Mano";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotea la mano del rival";

    [SerializeField] private Sprite cardIcon;
    [SerializeField] private int energyCost = 20;
    public CardType Type => CardType.Utility;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;

    [Header("Effect Settings")]
    public float effectDuration = 1f;
    public float floatGravity = -0.3f;

    public bool CanBeUsed(PlayerController user)
    {
        return true;
    }

    public void ExecuteCard(PlayerController character)
    {
        PlayerController rival = GetRival(character);

        if (rival != null)
        {
            AntiGravityEffect debuff = rival.gameObject.AddComponent<AntiGravityEffect>();
            debuff.StartEffect(effectDuration, floatGravity);
        }
    }

    private PlayerController GetRival(PlayerController user)
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
        {
            if (p != user) return p;
        }
        return null;
    }

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }
}
