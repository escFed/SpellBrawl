using UnityEngine;

public static class AerialInput
{
    public const float DiagonalComponentThreshold = 0.45f;

    public static bool IsDownDiagonal(Vector2 direction)
    {
        return Mathf.Abs(direction.x) >= DiagonalComponentThreshold && direction.y <= -DiagonalComponentThreshold;
    }

    public static bool IsFastFall(Vector2 direction, float downThreshold)
    {
        return direction.y < -downThreshold && Mathf.Abs(direction.x) < DiagonalComponentThreshold;
    }
}
