using UnityEngine;

public static class AnalyticsHelper

{ 

    public static bool CanSendMetrics()
    {
        return MetricsConsentManager.Instance != null && MetricsConsentManager.Instance.GetConsent();
    }

}
