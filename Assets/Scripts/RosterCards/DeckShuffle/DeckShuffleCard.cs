using UnityEngine;
using UnityEngine.UI;

public class DeckShuffleCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "DeckShuffle";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotages the opponent hand";
    [SerializeField] private string damageOrNot = "no";

    // Campo serializado para asignar en el Inspector
    [SerializeField] private Image cardVisual;

    [SerializeField] private Sprite cardIcon;
    [SerializeField] private int energyCost = 20;
    public CardType Type => CardType.Utility;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;

    // Implementación de la propiedad de la interfaz
    public Sprite CardVisual => cardIcon;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }

    public bool CanBeUsed(CharacterCoordinator user) => true;

    public void ExecuteCard(CharacterCoordinator character)
    {
        CharacterCoordinator rival = GetRival(character);

        if (rival != null)
        {
            CharacterDeck rivalDeck = rival.GetComponent<CharacterDeck>();
            if (rivalDeck != null)
            {
                rivalDeck.ForceSabotageRedraw();
                Debug.Log("¡Mano del rival saboteada!");
            }
        }
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
}
