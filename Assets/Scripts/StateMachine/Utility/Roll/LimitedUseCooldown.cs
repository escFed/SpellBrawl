using System;

public class LimitedUseCooldown
{
    private float CooldownEpsilon = 0.0001f;
    public int MaxUses { get; }
    public float CooldownDuration { get; }
    public int RemainingUses { get; private set; }
    public float CooldownRemaining { get; private set; }
    public bool IsCooldownActive => CooldownRemaining > 0f;
    public bool CanUse => RemainingUses > 0 && !cooldownPending && !IsCooldownActive;
    private bool cooldownPending;

    public LimitedUseCooldown(int maxUses, float cooldownDuration)
    {
        if (maxUses < 1)
            throw new ArgumentOutOfRangeException(nameof(maxUses));

        if (cooldownDuration < 0f)
            throw new ArgumentOutOfRangeException(nameof(cooldownDuration));

        MaxUses = maxUses;
        CooldownDuration = cooldownDuration;
        Reset();
    }

    public bool TryConsume()
    {
        if (!CanUse)
            return false;

        RemainingUses--;
        cooldownPending = RemainingUses == 0;
        return true;
    }

    public void CompleteUse()
    {
        if (!cooldownPending)
            return;

        cooldownPending = false;

        if (CooldownDuration <= 0f)
        {
            Reset();
            return;
        }

        CooldownRemaining = CooldownDuration;
    }

    public void Tick(float deltaTime)
    {
        if (!IsCooldownActive || deltaTime <= 0f)
            return;

        CooldownRemaining = Math.Max(0f, CooldownRemaining - deltaTime);

        if (CooldownRemaining <= CooldownEpsilon)
        {
            CooldownRemaining = 0f;
            RemainingUses = MaxUses;
        }
    }

    public void Reset()
    {
        RemainingUses = MaxUses;
        CooldownRemaining = 0f;
        cooldownPending = false;
    }
}
