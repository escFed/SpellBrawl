using UnityEngine;


public class MatchHitStats : MonoBehaviour
{
    public static int HitsRegisteredToPlayer { get; private set; }

    public static int HitsRegisteredtoAI { get; private set; }

    public static float StartTime { get; private set; }


    public static void ResetStats()
    {
        HitsRegisteredToPlayer = 0;
        HitsRegisteredtoAI = 0;
        StartTime = Time.time;
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
