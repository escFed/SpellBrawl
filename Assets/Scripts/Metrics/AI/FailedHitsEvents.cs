using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class FailedHitsEvents : MonoBehaviour
{

    public const string FailedHitsEventName = "golpes_fallidos";
    public static async Task Report(int failedHits, string attackerType)
    {
        await Report(failedHits, attackerType, MatchHitStats.MatchId);
    }

    public static async Task Report(int failedHits, string attackerType, string matchId)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI)
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            FailedHitsEventName,
            ("failed_hits", failedHits.ToString(CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType),
            ("match_id", matchId ?? string.Empty));
    }

}
