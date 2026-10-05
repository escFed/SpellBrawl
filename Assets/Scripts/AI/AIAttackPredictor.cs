using UnityEngine;

public static class AIAttackPredictor
{
    public static bool CanHit(Vector2 selfPosition, Vector2 selfVelocity, Vector2 targetPosition, Vector2 targetVelocity, float startup, float horizontalRange, float verticalRange)
    {
        float horizon = Mathf.Clamp(startup, 0f, 0.8f);
        Vector2 separation = targetPosition - selfPosition + (targetVelocity - selfVelocity) * horizon;
        return Mathf.Abs(separation.x) <= horizontalRange && Mathf.Abs(separation.y) <= verticalRange;
    }
}
