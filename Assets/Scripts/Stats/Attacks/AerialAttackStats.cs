using UnityEngine;

[CreateAssetMenu(fileName = "AerialAttackStats", menuName = "Character/Attacks/Aerial Attack")]
public class AerialAttackStats : NormalAttackStats
{
    [Header("Aerial Hit Suspension")]
    [Min(0f)] public float attackerSuspensionDuration = 0.1f;
    [Range(0f, 1f)] public float attackerGravityMultiplier = 0.4f;
    [Tooltip("Positive downward-speed limit applied to the attacker on hit. Zero preserves its current fall speed.")]
    [Min(0f)] public float attackerMaximumDownwardSpeed = 1.5f;
    [Min(0f)] public float defenderSuspensionDuration = 0.12f;
    [Range(0f, 1f)] public float defenderGravityMultiplier = 0.5f;

    [Header("Aerial Combo Decay")]
    [Range(0f, 1f)] public float suspensionDecay = 0.55f;
    [Min(1)] public int maximumSuspensionHits = 2;

    public float CalculateSuspensionStrength(int previousAerialHits)
    {
        if (previousAerialHits < 0 || previousAerialHits >= Mathf.Max(1, maximumSuspensionHits))
            return 0f;

        return Mathf.Pow(Mathf.Clamp01(suspensionDecay), previousAerialHits);
    }

    public AerialSuspension CreateAttackerSuspension(float strength)
    {
        return new AerialSuspension(attackerSuspensionDuration, attackerGravityMultiplier,
            attackerMaximumDownwardSpeed, strength);
    }

    public AerialSuspension CreateDefenderSuspension(float strength)
    {
        return new AerialSuspension(defenderSuspensionDuration, defenderGravityMultiplier, 0f, strength);
    }
}
