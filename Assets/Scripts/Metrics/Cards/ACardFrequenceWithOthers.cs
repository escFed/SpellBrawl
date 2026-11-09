using System.Threading.Tasks;
using UnityEngine;

public class ACardFrequenceWithOthers : MonoBehaviour
{
    public static Task Report(
        MetricsConsentManager metrics,
        string matchId,
        string cardId,
        string otherCardId,
        string result,
        string characterId,
        string deckOwner)
    {
        return metrics.TrackEventAsync(
            CardPickRateEvent.CardCooccurrenceEventName,
            ("match_id", matchId),
            ("deck_owner", deckOwner),
            ("character_id", characterId ?? string.Empty),
            ("card_id", cardId),
            ("other_card_id", otherCardId),
            ("result", result));
    }
}
