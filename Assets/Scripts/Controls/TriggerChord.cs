public class TriggerChord
{
    private const float ChordGraceSeconds = 0.06f;
    private bool wasLeftHeld;
    private bool wasRightHeld;
    private bool requiresRelease;
    private bool parryPending;
    private float pendingTime;

    public bool ShieldHeld { get; private set; }

    public bool Update(bool leftHeld, bool rightHeld, float deltaTime)
    {
        if (requiresRelease)
        {
            requiresRelease = leftHeld || rightHeld;
            wasLeftHeld = leftHeld;
            wasRightHeld = rightHeld;
            return false;
        }

        bool newPress = (leftHeld && !wasLeftHeld) || (rightHeld && !wasRightHeld);
        wasLeftHeld = leftHeld;
        wasRightHeld = rightHeld;
        ShieldHeld = leftHeld && rightHeld;

        if (ShieldHeld)
        {
            parryPending = false;
            return false;
        }

        if (newPress)
        {
            parryPending = true;
            pendingTime = 0f;
        }

        if (!parryPending)
            return false;

        pendingTime += deltaTime > 0f ? deltaTime : 0f;
        if ((leftHeld || rightHeld) && pendingTime < ChordGraceSeconds)
            return false;

        parryPending = false;
        return true;
    }

    public void Reset(bool leftHeld, bool rightHeld)
    {
        wasLeftHeld = leftHeld;
        wasRightHeld = rightHeld;
        requiresRelease = leftHeld || rightHeld;
        parryPending = false;
        pendingTime = 0f;
        ShieldHeld = false;
    }
}
