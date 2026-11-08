using System.Threading.Tasks;
using TaloGameServices;
using UnityEngine;

/// <summary>
/// Envía a Talo los eventos necesarios para calcular el winrate de la IA:
///   winrate = ia_victoria / ia_partida_jugada
/// Se llama una sola vez por partida, cuando termina (ver GameManager.ShowVictoryScreen).
/// </summary>
public static class IAWinrateEvent
{
   
    public const string PlayedEvent = "ia_partida_jugada";
    public const string FinishedEvent = "ia_victoria";

    public static async Task Report(PlayerSlot winner)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode) return;

        // Solo cuenta Jugador vs IA. En PvP no hay IA y en IA vs IA siempre
        // hay una victoria y una derrota de la IA, lo que distorsiona el winrate.
        if (selection.matchMode != MatchMode.PlayerVsAI) return;

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics == null || !metrics.GetConsent()) return;

        bool aiWon = selection.GetControlMode(winner) == PlayerMode.AI;

        
        await metrics.TrackEventAsync(PlayedEvent);
        if (aiWon)
            await metrics.TrackEventAsync(FinishedEvent);
    }
}
