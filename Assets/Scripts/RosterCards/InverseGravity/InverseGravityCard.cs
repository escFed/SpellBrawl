using UnityEngine;
using UnityEngine.UI;

public class InverseGravityCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "InverseGravity Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Inversely affects gravity for a short duration.";
    [SerializeField] private string damageOrNot = "no";

    [SerializeField] private Sprite cardIcon;
    [SerializeField] private int energyCost = 20;
    public CardType Type => CardType.Utility;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;

    // Campo añadido para referencia del Image de UI
    [SerializeField] private Image cardVisual;
    // Implementación requerida por ICardable: devuelve el Sprite (el icono)
    public Sprite CardVisual => cardIcon;
    // Propiedad adicional para exponer el Image si es necesaria en el resto del código
    public Image CardVisualImage => cardVisual;

    [Header("Effect Settings")]
    public float effectDuration = 1f;
    public float floatGravity = -0.3f;

    public bool CanBeUsed(CharacterCoordinator user)
    {
        return true;
    }

    public void ExecuteCard(CharacterCoordinator character)
    {
        CharacterCoordinator rival = GetRival(character);

        if (rival != null)
        {
            AntiGravityEffect debuff = rival.gameObject.AddComponent<AntiGravityEffect>();
            debuff.StartEffect(effectDuration, floatGravity);
        }

        Destroy(gameObject);
    }

    private CharacterCoordinator GetRival(CharacterCoordinator user)
    {
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator p in allPlayers)
        {
            if (p != user) return p;
        }
        return null;
    }

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
        // Mantener referencia al Image de UI para la propiedad CardVisualImage
        if (img != null) cardVisual = img;
    }
}
