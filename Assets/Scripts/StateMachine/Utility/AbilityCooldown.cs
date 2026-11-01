using System;

public class AbilityCooldown
{
    private const float CooldownEpsilon = 0.0001f;

    public float CooldownDuration { get; }
    public float CooldownRemaining { get; private set; }
    public bool CanUse => !inUse && CooldownRemaining <= 0f;

    private bool inUse;

    public AbilityCooldown(float cooldownDuration)
    {
        if (cooldownDuration < 0f)
            throw new ArgumentOutOfRangeException(nameof(cooldownDuration));

        CooldownDuration = cooldownDuration;
    }

    public bool TryStart()
    {
        if (!CanUse)
            return false;

        inUse = true;
        return true;
    }

    public void CompleteUse()
    {
        if (!inUse)
            return;

        inUse = false;
        CooldownRemaining = CooldownDuration;
    }

    public void Tick(float deltaTime)
    {
        if (CooldownRemaining <= 0f || deltaTime <= 0f)
            return;

        CooldownRemaining = Math.Max(0f, CooldownRemaining - deltaTime);

        if (CooldownRemaining <= CooldownEpsilon)
            CooldownRemaining = 0f;
    }

    public void Reset()
    {
        inUse = false;
        CooldownRemaining = 0f;
    }
}
