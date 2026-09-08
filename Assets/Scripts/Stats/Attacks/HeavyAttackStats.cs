using UnityEngine;

[CreateAssetMenu(fileName = "HeavyAttackStats", menuName = "Character/Attacks/Heavy Attack")]
public class HeavyAttackStats : AttackStats
{
    [Header("Charge")]
    public float maxChargeTime = 2f;

    [Header("Charged Hit Data")]
    public float minDamage = 10f;
    public float maxDamage = 20f;
    public float minKnockback = 4f;
    public float maxKnockback = 10f;
    [Min(0f)] public float maxKnockbackGrowth = 14f;
    public Vector2 knockbackDirection = new Vector2(1f, 0.5f);

    [Header("Charged Hit Stun")]
    [InspectorName("Fully Charged Base Hit Stun")] public float maxHitStun = 0.85f;

    [Header("Animation")]
    public string chargeAnimationState = "HeavyCharge";
    public string executionAnimationState = "HeavyAttack";
}
