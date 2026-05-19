using UnityEngine;
using UnityEngine.UI;

public class DeckShuffleCard : MonoBehaviour, ICardable
{
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private int energyCost = 20;
    public CardType Type => CardType.Utility;
    public int EnergyCost => energyCost;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        PlayerController rival = GetRival(character);

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

    private PlayerController GetRival(PlayerController user)
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
        {
            if (p != user) return p;
        }
        return null;
    }
}
