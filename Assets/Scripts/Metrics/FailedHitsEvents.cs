using UnityEngine;

public class FailedHitsEvents : MonoBehaviour
{

    public const string FailedHitsEventName = "golpes_fallidos";
    public static async void Report(int failedHits)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;
        await metrics.TrackEventAsync(
            FailedHitsEventName,
            ("failed_hits", failedHits.ToString()));
    }

}
