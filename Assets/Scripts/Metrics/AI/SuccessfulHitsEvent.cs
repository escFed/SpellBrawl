using UnityEngine;

public class SuccessfulHitsEvent : MonoBehaviour
{

    public const string SuccessfulHitsEventName = "golpes_exitosos";
    public static async void Report(int successfulHits)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            SuccessfulHitsEventName,
            ("successful_hits", successfulHits.ToString()));
    }

}
