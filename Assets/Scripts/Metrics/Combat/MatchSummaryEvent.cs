using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public static class MatchSummaryEvent
{
    public const string EventName = "match_summary";
    public const string CharacterMatchResultEventName = "character_match_result";

    public static async Task Report(int hitsPlayer, int hitsAi, int attemptsPlayer, int attemptsAi, float durationSeconds)
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

    public static async Task ReportCharacterDuration(
        CharacterCoordinator character,
        bool won,
        float durationSeconds,
        string matchId)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode ||
            character == null || character.Stats == null || string.IsNullOrWhiteSpace(character.Stats.MetricId) ||
            string.IsNullOrWhiteSpace(matchId))
            return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent())
            return;

        await metrics.TrackEventAsync(
            CharacterMatchResultEventName,
            ("match_id", matchId),
            ("character_id", character.Stats.MetricId),
            ("duration_seconds", Mathf.RoundToInt(durationSeconds).ToString(CultureInfo.InvariantCulture)),
            ("result", won ? "win" : "loss"),
            ("player_type", character.Mode == PlayerMode.AI ? "ai" : "player"),
            ("player_slot", character.Slot.ToString()),
            ("match_mode", selection.matchMode.ToString()));
    }
}