using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public static class CardPickRateEvent
{
    public const string CardPickRateEventName = "card_pick_rate";
    public const string CardWinRateEventName = "a_card_win_rate";
    public const string CardCooccurrenceEventName = "a_card_frequence_with_others";

    public static async Task ReportMatch(
        IReadOnlyList<GameObject> deck,
        PlayerSlot deckSlot,
        bool won,
        string matchId,
        string characterId = null)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI ||
            deck == null || deck.Count == 0 || string.IsNullOrWhiteSpace(matchId))
            return;

        PlayerMode deckMode = selection.GetControlMode(deckSlot);
        if (deckMode != PlayerMode.Player && deckMode != PlayerMode.AI)
            return;
        string deckOwner = deckMode == PlayerMode.AI ? "ai" : "player";

        if (string.IsNullOrWhiteSpace(characterId))
        {
            int characterIndex = selection.GetSelectedCharacter(deckSlot);
            if (selection.characterDb != null &&
                characterIndex >= 0 && characterIndex < selection.characterDb.CharacterCount)
            {
                CharacterStats character = selection.characterDb.GetCharacter(characterIndex);
                characterId = character != null ? character.MetricId : null;
            }
        }

        if (string.IsNullOrWhiteSpace(characterId))
        {
            Debug.LogError("[CardPickRateEvent] The selected character has no metric ID.");
            return;
        }

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

            cards.Add(new CardMetricEntry(cardId, card.Type.ToString().ToLowerInvariant()));
        }

        cards.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        string result = won ? "win" : "loss";
        foreach (CardMetricEntry card in cards)
        {
            await metrics.TrackEventAsync(
                CardPickRateEventName,
                ("match_id", matchId),
                ("deck_owner", deckOwner),
                ("character_id", characterId),
                ("card_id", card.Id),
                ("card_type", card.Type),
                ("deck_size", cards.Count.ToString(CultureInfo.InvariantCulture)));
            await ACardWinRate.Report(metrics, matchId, card.Id, result, characterId, deckOwner, card.Type, cards.Count);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = i + 1; j < cards.Count; j++)
                await ACardFrequenceWithOthers.Report(
                    metrics, matchId, cards[i].Id, cards[j].Id, result, characterId, deckOwner);
        }
    }

    private readonly struct CardMetricEntry
    {
        public string Id { get; }
        public string Type { get; }

        public CardMetricEntry(string id, string type)
        {
            Id = id;
            Type = type;
        }
    }
}
