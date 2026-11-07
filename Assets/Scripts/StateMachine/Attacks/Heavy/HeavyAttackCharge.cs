using UnityEngine;

public class HeavyAttackCharge
{
    // A short press is an uncharged attack; holding past this point builds power.
    public const float TapThreshold = 0.15f;

    public float ChargeTime { get; private set; }
    public float MaxChargeTime { get; private set; } = 2f;
    public bool IsCharging { get; private set; }
    public float ChargeRatio => Normalize(Mathf.Max(0f, ChargeTime - TapThreshold),
        Mathf.Max(0.01f, MaxChargeTime - TapThreshold));
    public bool IsFullyCharged => IsCharging && ChargeTime >= MaxChargeTime;

    public void Begin(float maxChargeTime)
    {
        MaxChargeTime = Mathf.Max(TapThreshold + 0.01f, maxChargeTime);
        ChargeTime = 0f;
        IsCharging = true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsCharging)
            return;

        ChargeTime = Mathf.Min(MaxChargeTime, ChargeTime + Mathf.Max(0f, deltaTime));
    }

    public void Reset()
    {
        ChargeTime = 0f;
        IsCharging = false;
    }

    public static float Normalize(float chargeTime, float maxChargeTime)
    {
        return Mathf.Clamp01(chargeTime / Mathf.Max(0.01f, maxChargeTime));
    }

    public static float CalculateDamage(float minDamage, float maxDamage, float chargeRatio)
    {
        return Mathf.Lerp(minDamage, maxDamage, Mathf.Clamp01(chargeRatio));
    }

    public static float CalculateKnockback(float minKnockback, float maxKnockback, float chargeRatio)
    {
        return Mathf.Lerp(minKnockback, maxKnockback, Mathf.Clamp01(chargeRatio));
    }

    public static float CalculateHitStun(float minHitStun, float maxHitStun, float chargeRatio)
    {
        return Mathf.Lerp(minHitStun, maxHitStun, Mathf.Clamp01(chargeRatio));
    }
}
