using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class CardPickRateEvent
{
    public const string CardWinRateEventName = "a_card_win_rate";
    public const string CardCooccurrenceEventName = "a_card_frequence_with_others";

    public static async Task ReportMatch(
        IReadOnlyList<GameObject> deck,
        PlayerSlot deckSlot,
        bool won,
        string matchId)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI ||
            selection.GetControlMode(deckSlot) != PlayerMode.Player ||
            deck == null || deck.Count == 0 || string.IsNullOrWhiteSpace(matchId))
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent())
            return;

        List<CardMetricEntry> cards = new List<CardMetricEntry>(deck.Count);
        HashSet<string> cardIds = new HashSet<string>();
        for (int i = 0; i < deck.Count; i++)
        {
            GameObject cardPrefab = deck[i];
            ICardable card = cardPrefab != null ? cardPrefab.GetComponent<ICardable>() : null;
            if (card == null)
            {
                Debug.LogError($"[CardPickRateEvent] Deck entry {i} has no card data.");
                continue;
            }

            string cardId = string.IsNullOrWhiteSpace(card.CardName)
                ? cardPrefab.name
                : card.CardName.Trim();
            if (!cardIds.Add(cardId))
            {
                Debug.LogError($"[CardPickRateEvent] Deck contains duplicate card id '{cardId}'.");
                continue;
            }

            cards.Add(new CardMetricEntry(cardId));
        }

        cards.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        string result = won ? "win" : "loss";
        foreach (CardMetricEntry card in cards)
            await ACardWinRate.Report(metrics, matchId, card.Id, result);

        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = i + 1; j < cards.Count; j++)
                await ACardFrequenceWithOthers.Report(
                    metrics, matchId, cards[i].Id, cards[j].Id, result);
        }
    }

    private readonly struct CardMetricEntry
    {
        public string Id { get; }

        public CardMetricEntry(string id)
        {
            Id = id;
        }
    }
}
