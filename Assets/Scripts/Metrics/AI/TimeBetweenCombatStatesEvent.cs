using System.Globalization;
using TaloGameServices;
using UnityEngine;

public class TimeBetweenCombatStatesEvent : MonoBehaviour
{
    public const string TimeBetweenCombatStatesEventName = "time_between_combat_states";

    public static async void Report(float combatTimePercentage, float neutralTimePercentage)
    {
        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        await metrics.TrackEventAsync(
            TimeBetweenCombatStatesEventName,
            ("combat_time_percentage", combatTimePercentage.ToString("F2", CultureInfo.InvariantCulture)),
            ("neutral_time_percentage", neutralTimePercentage.ToString("F2", CultureInfo.InvariantCulture)));
    }
}
