using System;
using System.Collections.Generic;
using System.Globalization;
using TaloGameServices;
using Unity.VisualScripting;
using UnityEngine;
using static MatchStatistics;
public static class AveragePlayersLifesEvents
{

    public const string PlayerWinLivesEvent = "vidas_restantes_victoria_jugador";
    public const string AIWinLivesEvent = "vidas_restantes_victoria_ia";

    public static async void Report(float livesLeft, bool aiWon)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode) return;
        if (selection.matchMode != MatchMode.PlayerVsAI) return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        string eventName = aiWon ? AIWinLivesEvent : PlayerWinLivesEvent;
        int lives = Mathf.RoundToInt(livesLeft);

        await metrics.TrackEventAsync(
            eventName,
            ("vidas_restantes", lives.ToString(CultureInfo.InvariantCulture)));
    }

}
