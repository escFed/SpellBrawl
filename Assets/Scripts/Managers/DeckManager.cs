using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    private List<GameObject> selectedDeck = new List<GameObject>();

    public IReadOnlyList<GameObject> SelectedDeck => selectedDeck;

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

    public bool TrySetDeck(IReadOnlyList<GameObject> cards, int requiredSize)
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

        selectedDeck.Clear();
        selectedDeck.AddRange(cards);
        return true;
    }
}
