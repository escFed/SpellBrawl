using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCatalog", menuName = "Cards/Card Catalog")]
public class CardCatalog : ScriptableObject
{
    [SerializeField] private GameObject[] cards;

    public bool Contains(GameObject cardPrefab)
    {
        if (cardPrefab == null || cards == null)
            return false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == cardPrefab)
                return cardPrefab.GetComponent<ICardable>() != null;
        }

        return false;
    }

    public int CopyValidUniqueCardsTo(List<GameObject> destination)
    {
        destination.Clear();

        if (cards == null)
            return 0;

        HashSet<GameObject> uniqueCards = new HashSet<GameObject>();
        for (int i = 0; i < cards.Length; i++)
        {
            GameObject card = cards[i];
            if (card != null && card.GetComponent<ICardable>() != null && uniqueCards.Add(card))
                destination.Add(card);
        }

        return destination.Count;
    }
}
