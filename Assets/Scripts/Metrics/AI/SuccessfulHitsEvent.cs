using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class SuccessfulHitsEvent : MonoBehaviour
{

    public const string SuccessfulHitsEventName = "golpes_exitosos";
    public static async Task Report(int successfulHits)
    {
        await Report(successfulHits, "player", MatchHitStats.MatchId);
    }

    public static async Task Report(int successfulHits, string attackerType, string matchId)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI)
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            SuccessfulHitsEventName,
            ("successful_hits", successfulHits.ToString(CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType),
            ("match_id", matchId ?? string.Empty));
    }

}
