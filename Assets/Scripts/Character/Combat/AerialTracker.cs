using UnityEngine;

public class AerialTracker
{
    private object target;
    private int aerialHits;
    private float expiresAt;

    public int AerialHits => aerialHits;

    public void Begin(object newTarget, float currentTime, float window)
    {
        target = newTarget;
        aerialHits = 0;
        expiresAt = currentTime + Mathf.Max(0f, window);
    }

    public float Preview(object candidate, float currentTime, bool targetGrounded, int maximumHits, float decay)
    {
        int previousHits = IsContinuation(candidate, currentTime, targetGrounded) ? aerialHits : 0;
        if (previousHits >= Mathf.Max(1, maximumHits))
            return 0f;

        return Mathf.Pow(Mathf.Clamp01(decay), previousHits);
    }

    public void Confirm(object candidate, float currentTime, bool targetGrounded, float window)
    {
        if (!IsContinuation(candidate, currentTime, targetGrounded))
            aerialHits = 0;

        target = candidate;
        aerialHits++;
        expiresAt = currentTime + Mathf.Max(0f, window);
    }

    public void Reset()
    {
        target = null;
        aerialHits = 0;
        expiresAt = 0f;
    }

    private bool IsContinuation(object candidate, float currentTime, bool targetGrounded)
    {
        return !targetGrounded && ReferenceEquals(target, candidate) && currentTime <= expiresAt;
    }
}