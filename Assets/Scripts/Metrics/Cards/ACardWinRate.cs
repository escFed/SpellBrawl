using System.Threading.Tasks;
using UnityEngine;

public class ACardWinRate : MonoBehaviour
{
    public static Task Report(
        MetricsConsentManager metrics,
        string matchId,
        string cardId,
        string result,
        string characterId,
        string deckOwner,
        string cardType,
        int deckSize)
    {
        return metrics.TrackEventAsync(
            CardPickRateEvent.CardWinRateEventName,
            ("match_id", matchId),
            ("deck_owner", deckOwner),
            ("character_id", characterId ?? string.Empty),
            ("card_id", cardId),
            ("card_type", cardType ?? string.Empty),
            ("deck_size", deckSize.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("result", result));
    }
}
