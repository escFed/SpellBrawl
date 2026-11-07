using System.Globalization;
using UnityEngine;

public static class MatchSummaryEvent
{
    public const string EventName = "match_summary";

    public static async void Report(int hitsPlayer, int hitsAi, float durationSeconds)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        float minutes = Mathf.Max(durationSeconds / 60f, 0.01f);
        float hitsPerMinute = hitsPlayer / minutes;

        // Los cortes salen de la regla de tu profe (100 golpes en 3 min ≈ 33/min).
        // Son un punto de partida: ajustalos cuando tengas datos reales.
        string profile = hitsPerMinute > 23f ? "aggressive"
                       : hitsPerMinute < 10f ? "passive"
                       : "average";

        await metrics.TrackEventAsync(
            EventName,
            ("hits_player", hitsPlayer.ToString(CultureInfo.InvariantCulture)),
            ("hits_ai", hitsAi.ToString(CultureInfo.InvariantCulture)),
            ("duration_seconds", Mathf.RoundToInt(durationSeconds).ToString(CultureInfo.InvariantCulture)),
            ("hits_per_minute", hitsPerMinute.ToString("0", CultureInfo.InvariantCulture)),
            ("player_profile", profile));
    }
}