using System.Globalization;
using System.Threading.Tasks;

public static class HitAdditionToKnockBack
{
    public const string EventName = "hit_added_to_knockback";

    public static async Task Report(float knockbackAdded, int damage, string attackerType, string matchId)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI)
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        await metrics.TrackEventAsync(
            EventName,
            ("knockback_added", knockbackAdded.ToString("0.#", CultureInfo.InvariantCulture)),
            ("damage", damage.ToString(CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType),
            ("match_id", matchId));
    }
}