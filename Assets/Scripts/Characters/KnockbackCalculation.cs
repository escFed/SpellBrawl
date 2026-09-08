using UnityEngine;

public static class KnockbackCalculation
{
    public static Vector2 CalculateVelocity(CombatHit hit, int damageAfterHit, float weight, Vector2 directionalInput = default, float maxInfluenceDegrees = 0f)
    {
        if (hit.BaseKnockback.sqrMagnitude < 0.000001f)
            return Vector2.zero;

        float speed = hit.BaseKnockback.magnitude + hit.Growth * Mathf.Max(0, damageAfterHit) / 100f;
        Vector2 velocity = hit.BaseKnockback.normalized * speed * (100f / Mathf.Max(1f, weight));
        return ApplyDirectionalInfluence(velocity, directionalInput, maxInfluenceDegrees * hit.DirectionalInfluence);
    }

    public static Vector2 ApplyDirectionalInfluence(Vector2 velocity, Vector2 input, float maxDegrees)
    {
        const float deadzone = 0.2f;
        float strength = Mathf.Clamp01(input.magnitude);

        if (velocity.sqrMagnitude < 0.000001f || strength <= deadzone || maxDegrees <= 0f)
            return velocity;

        Vector2 direction = velocity.normalized;
        Vector2 stick = input.normalized * ((strength - deadzone) / (1f - deadzone));
        float perpendicular = direction.x * stick.y - direction.y * stick.x;
        float radians = perpendicular * Mathf.Clamp(maxDegrees, 0f, 25f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(velocity.x * cos - velocity.y * sin, velocity.x * sin + velocity.y * cos);
    }

    public static float CalculateHitStun(CombatHit hit, Vector2 launchVelocity)
    {
        return Mathf.Min(hit.MaxHitStun, hit.BaseHitStun + launchVelocity.magnitude * hit.HitStunPerSpeed);
    }

    public static Vector2 RemoveIntoSurface(Vector2 velocity, Vector2 normal)
    {
        return velocity - normal * Mathf.Min(0f, Vector2.Dot(velocity, normal));
    }
}