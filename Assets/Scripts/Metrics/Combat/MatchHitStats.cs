using UnityEngine;


public class MatchHitStats : MonoBehaviour
{
    public static int HitsRegisteredToPlayer { get; private set; }

    public static int HitsRegisteredtoAI { get; private set; }

    public static float StartTime { get; private set; }

    public static int AttemptsToPlayer { get; private set; }
    public static int AttemptsToAI { get; private set; }

    public static void ResetStats()
    {
        HitsRegisteredToPlayer = 0;
        HitsRegisteredtoAI = 0;
        AttemptsToPlayer = 0;
        AttemptsToAI = 0;
        StartTime = Time.time;
    }



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
