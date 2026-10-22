using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCategory", menuName = "Cards/Card Category")]
public class CardCategoryCatalog : ScriptableObject
{
    [SerializeField] private CardType type;
    [SerializeField] private GameObject[] cards;

    public CardType Type => type;

    public bool Contains(GameObject cardPrefab)
    {
        if (cardPrefab == null || cards == null)
            return false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == cardPrefab)
                return IsValidCard(cardPrefab);
        }

        return false;
    }

    public void CopyValidUniqueCardsTo(List<GameObject> destination, HashSet<GameObject> uniqueCards)
    {
        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            GameObject card = cards[i];
            if (IsValidCard(card) && uniqueCards.Add(card))
                destination.Add(card);
        }
    }

    private bool IsValidCard(GameObject cardPrefab)
    {
        return cardPrefab != null &&
            cardPrefab.GetComponent<ICardable>() is ICardable card && card.Type == type;
    }
}
