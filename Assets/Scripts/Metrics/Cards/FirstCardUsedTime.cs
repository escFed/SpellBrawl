using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class FirstCardUsedTime : MonoBehaviour
{
    public const string EventName = "first_card_used_time";

    private static float matchStartTime;
    private static bool matchTimerStarted;
    private static bool firstUseReported;
    private static Task pendingReport = Task.CompletedTask;

    public static void BeginMatchTimer()
    {
        matchStartTime = Time.time;
        matchTimerStarted = true;
        firstUseReported = false;
        pendingReport = Task.CompletedTask;
    }

    public static void RecordFirstUse(CharacterCoordinator character)
    {
        if (firstUseReported || !matchTimerStarted || character == null)
            return;

        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI ||
            selection.GetControlMode(character.Slot) != PlayerMode.Player)
            return;

        firstUseReported = true;
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent())
            return;

        float elapsedSeconds = Mathf.Max(0f, Time.time - matchStartTime);
        pendingReport = ReportAsync(metrics, elapsedSeconds, MatchHitStats.MatchId);
    }

    public static Task WaitForPendingReportAsync()
    {
        return pendingReport;
    }

    private static Task ReportAsync(MetricsConsentManager metrics, float elapsedSeconds, string matchId)
    {
        return metrics.TrackEventAsync(
            EventName,
            ("match_id", matchId ?? string.Empty),
            ("seconds_to_first_card", elapsedSeconds.ToString("0.###", CultureInfo.InvariantCulture)));
    }
}
