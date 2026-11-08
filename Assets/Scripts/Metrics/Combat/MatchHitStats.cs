using System;
using UnityEngine;


public class MatchHitStats : MonoBehaviour
{
    public static int HitsRegisteredToPlayer { get; private set; }

    public static int HitsRegisteredtoAI { get; private set; }

    public static float StartTime { get; private set; }
    public static string MatchId { get; private set; }

    public static int AttemptsToPlayer { get; private set; }
    public static int AttemptsToAI { get; private set; }
    public static float CombatTimePercentage { get; private set; }
    public static float NeutralTimePercentage { get; private set; }

    private static float combatDuration;
    private static float neutralDuration;
    private static bool isRecording;

    public static void ResetStats()
    {
        HitsRegisteredToPlayer = 0;
        HitsRegisteredtoAI = 0;
        AttemptsToPlayer = 0;
        AttemptsToAI = 0;
        combatDuration = 0f;
        neutralDuration = 0f;
        CombatTimePercentage = 0f;
        NeutralTimePercentage = 0f;
        isRecording = true;
        StartTime = Time.time;
    }

    public static void BeginMatch()
    {
        MatchId = Guid.NewGuid().ToString();
    }

    public static void RecordStateTime(CharacterCoordinator character, float duration)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (!isRecording || character == null || character.Mode != PlayerMode.AI ||
            selection == null || selection.isTrainingMode || selection.matchMode != MatchMode.PlayerVsAI || duration <= 0f)
            return;

        ICharacterState state = character.GetCurrentState();
        bool isCombatState = state != null && state != character.States.Idle &&
            state != character.States.Move && state != character.States.Jump && state != character.States.Die;
        if (isCombatState)
            combatDuration += duration;
        else
            neutralDuration += duration;

        float totalDuration = combatDuration + neutralDuration;
        CombatTimePercentage = totalDuration > 0f ? 100f * combatDuration / totalDuration : 0f;
        NeutralTimePercentage = totalDuration > 0f ? 100f * neutralDuration / totalDuration : 0f;
    }

    public static void StopRecording() => isRecording = false;

    public static void AttemptRegister(string attackerType)
    {
        if (attackerType == "player") AttemptsToPlayer++;
        else if (attackerType == "ai") AttemptsToAI++;
    }

    public static void HitRegister(string attackerType)
    {
        if (attackerType == "player") HitsRegisteredToPlayer++;
        else if (attackerType == "ai") HitsRegisteredtoAI++;
    }



    public static float GetMatchDuration()
    {
        return Time.time - StartTime;
    }
}
