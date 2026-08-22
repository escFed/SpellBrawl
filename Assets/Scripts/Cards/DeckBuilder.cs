using System;
using System.Collections.Generic;
using UnityEngine;

public static class DeckBuilder
{
    public static bool TryBuild(IReadOnlyList<GameObject> selectedCards, CardCatalog catalog, int deckSize, List<GameObject> destination, out int availableCardCount)
    {
        destination.Clear();

        List<GameObject> candidates = new List<GameObject>();
        if (selectedCards != null && selectedCards.Count > 0)
        {
            CopySelectedCards(selectedCards, catalog, candidates);
        }
        else if (catalog != null)
        {
            catalog.CopyValidUniqueCardsTo(candidates);
        }

        availableCardCount = candidates.Count;
        if (availableCardCount < deckSize)
            return false;

        Shuffle(candidates);
        for (int i = 0; i < deckSize; i++)
            destination.Add(candidates[i]);

        return true;
    }

    private static void CopySelectedCards(IReadOnlyList<GameObject> selectedCards, CardCatalog catalog, List<GameObject> destination)
    {
        HashSet<GameObject> uniqueCards = new HashSet<GameObject>();
        for (int i = 0; i < selectedCards.Count; i++)
        {
            GameObject candidate = selectedCards[i];
            if (catalog != null && catalog.Contains(candidate) && uniqueCards.Add(candidate))
                destination.Add(candidate);
        }
    }

    private static void Shuffle(List<GameObject> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }
}
