using System.Globalization;

public static class HitAdditionToKnockBack
{
    public const string EventName = "hit_added_to_knockback";

    public static async void Report(float knockbackAdded, int damage, string attackerType)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        await metrics.TrackEventAsync(
            EventName,
            ("knockback_added", knockbackAdded.ToString("0.#", CultureInfo.InvariantCulture)),
            ("damage", damage.ToString(CultureInfo.InvariantCulture)),
            ("attacker_type", attackerType));
    }
}