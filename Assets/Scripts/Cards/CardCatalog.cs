using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCatalog", menuName = "Cards/Card Catalog")]
public class CardCatalog : ScriptableObject
{
    [SerializeField] private CardCategoryCatalog[] categories;
    // Kept for existing catalogs that have not yet been moved into category assets.
    [SerializeField] private GameObject[] cards;

    public bool Contains(GameObject cardPrefab)
    {
        if (cardPrefab == null)
            return false;

        if (categories != null)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] != null && categories[i].Contains(cardPrefab))
                    return true;
            }
        }

        if (cards != null)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == cardPrefab)
                    return cardPrefab.GetComponent<ICardable>() != null;
            }
        }

        return false;
    }

    public int CopyValidUniqueCardsTo(List<GameObject> destination)
    {
        destination.Clear();

        HashSet<GameObject> uniqueCards = new HashSet<GameObject>();
        if (categories != null)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] != null)
                    categories[i].CopyValidUniqueCardsTo(destination, uniqueCards);
            }
        }

        if (cards != null)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                GameObject card = cards[i];
                if (card != null && card.GetComponent<ICardable>() != null && uniqueCards.Add(card))
                    destination.Add(card);
            }
        }

        return destination.Count;
    }
}
