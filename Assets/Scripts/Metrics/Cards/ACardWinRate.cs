using System.Threading.Tasks;
using UnityEngine;

public class ACardWinRate : MonoBehaviour
{
    public static Task Report(
        MetricsConsentManager metrics,
        string matchId,
        string cardId,
        string result)
    {
        return metrics.TrackEventAsync(
            CardPickRateEvent.CardWinRateEventName,
            ("match_id", matchId),
            ("deck_owner", "player"),
            ("card_id", cardId),
            ("result", result));
    }
}
