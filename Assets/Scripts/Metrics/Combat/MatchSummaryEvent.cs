using System.Globalization;
using UnityEngine;

public static class MatchSummaryEvent
{
    public const string EventName = "match_summary";

    public static async void Report(int hitsPlayer, int hitsAi, int attemptsPlayer, int attemptsAi, float durationSeconds)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        float minutes = Mathf.Max(durationSeconds / 60f, 0.01f);
        float attemptsPerMinute = attemptsPlayer / minutes;
        float accuracy = attemptsPlayer > 0 ? hitsPlayer * 100f / attemptsPlayer : 0f;

        // Cortes provisionales: ajustalos con datos reales.
        string profile = attemptsPerMinute > 23f ? "aggressive"
                       : attemptsPerMinute < 10f ? "passive"
                       : "average";

        await metrics.TrackEventAsync(
            EventName,
            ("attempts_player", attemptsPlayer.ToString(CultureInfo.InvariantCulture)),
            ("attempts_ai", attemptsAi.ToString(CultureInfo.InvariantCulture)),
            ("hits_player", hitsPlayer.ToString(CultureInfo.InvariantCulture)),
            ("hits_ai", hitsAi.ToString(CultureInfo.InvariantCulture)),
            ("duration_seconds", Mathf.RoundToInt(durationSeconds).ToString(CultureInfo.InvariantCulture)),
            ("attempts_per_minute", attemptsPerMinute.ToString("0", CultureInfo.InvariantCulture)),
            ("accuracy_percent", accuracy.ToString("0", CultureInfo.InvariantCulture)),
            ("player_profile", profile));
    }
}