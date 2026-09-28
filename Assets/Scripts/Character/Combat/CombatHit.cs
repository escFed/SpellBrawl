using UnityEngine;

// Snapshot the tuning when the attack is prepared; charged attacks can override growth.
public struct CombatHit
{
    public int Damage;
    public Vector2 BaseKnockback;
    public float Growth;
    public float BaseHitStun;
    public float HitStunPerSpeed;
    public float MaxHitStun;
    public float DirectionalInfluence;
    public float AirDecelerationMultiplier;
    public AerialSuspension DefenderSuspension;
    public HitReaction Reaction;
    public Vector2 Point;
    public int AttackerPlayerIndex;

    public CombatHit(int damage, Vector2 baseKnockback, float hitStun = 0.3f, HitReaction reaction = HitReaction.Hit, Vector2 point = default, int attackerPlayerIndex = -1, KnockbackProfile profile = null, float growthOverride = -1f, AerialSuspension defenderSuspension = default)
    {
        Damage = Mathf.Max(0, damage);
        BaseKnockback = baseKnockback;
        Growth = Mathf.Max(0f, growthOverride >= 0f ? growthOverride : profile?.growth ?? 3f);
        BaseHitStun = Mathf.Max(0f, hitStun);
        HitStunPerSpeed = Mathf.Max(0f, profile?.hitStunPerSpeed ?? 0.006f);
        MaxHitStun = Mathf.Max(0f, profile?.maxHitStun ?? 0.8f);
        DirectionalInfluence = Mathf.Clamp01(profile?.directionalInfluence ?? 1f);
        AirDecelerationMultiplier = Mathf.Clamp(profile?.airDecelerationMultiplier ?? 1f, 0.1f, 2f);
        DefenderSuspension = defenderSuspension;
        Reaction = reaction;
        Point = point;
        AttackerPlayerIndex = attackerPlayerIndex;
    }
}

public readonly struct AerialSuspension
{
    public float Duration { get; }
    public float GravityMultiplier { get; }
    public float MaximumDownwardSpeed { get; }
    public bool IsActive => Duration > 0f;

    public AerialSuspension(float duration, float gravityMultiplier, float maximumDownwardSpeed, float strength = 1f)
    {
        float clampedStrength = Mathf.Clamp01(strength);
        Duration = Mathf.Max(0f, duration) * clampedStrength;
        GravityMultiplier = Mathf.Lerp(1f, Mathf.Clamp01(gravityMultiplier), clampedStrength);
        MaximumDownwardSpeed = maximumDownwardSpeed > 0f && clampedStrength > 0f
            ? maximumDownwardSpeed / clampedStrength
            : 0f;
    }
}

