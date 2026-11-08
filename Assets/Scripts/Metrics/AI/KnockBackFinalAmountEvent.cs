using System.Threading.Tasks;
using UnityEngine;

public static class KnockBackFinalAmountEvent
{
    public const string EventName = "knockback_final_amount";
    public static async Task Report(float knockback, string attackerType, int damageAtDeath)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI)
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            EventName,
            ("knockback_final", knockback.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)),
            ("damage_at_death", damageAtDeath.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType));
    }
}