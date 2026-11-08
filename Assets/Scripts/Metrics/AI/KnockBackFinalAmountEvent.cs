using UnityEngine;

public static class KnockBackFinalAmountEvent
{
    public const string EventName = "knockback_final_amount";
    public static async void Report(float knockback, string attackerType)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            EventName,
            ("knockback_final", knockback.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType));
    }
}