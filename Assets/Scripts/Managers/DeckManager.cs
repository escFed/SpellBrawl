using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    private List<GameObject> p1SelectedDeck = new List<GameObject>();
    private List<GameObject> p2SelectedDeck = new List<GameObject>();

    public IReadOnlyList<GameObject> SelectedDeck => p1SelectedDeck;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool TrySetDeck(PlayerSlot slot, IReadOnlyList<GameObject> cards, int requiredSize)
    {
        if (cards == null || cards.Count != requiredSize)
            return false;

        HashSet<GameObject> uniqueCards = new HashSet<GameObject>();
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject cardPrefab = cards[i];
            if (cardPrefab == null || cardPrefab.GetComponent<ICardable>() == null || !uniqueCards.Add(cardPrefab))
                return false;
        }

        List<GameObject> targetDeck = GetMutableDeck(slot);
        targetDeck.Clear();
        targetDeck.AddRange(cards);
        return true;
    }

    public IReadOnlyList<GameObject> GetDeck(PlayerSlot slot)
    {
        return slot == PlayerSlot.PlayerOne ? p1SelectedDeck : p2SelectedDeck;
    }

    public void ClearDecks()
    {
        p1SelectedDeck.Clear();
        p2SelectedDeck.Clear();
    }

    private List<GameObject> GetMutableDeck(PlayerSlot slot)
    {
        return slot == PlayerSlot.PlayerOne ? p1SelectedDeck : p2SelectedDeck;
    }
}
