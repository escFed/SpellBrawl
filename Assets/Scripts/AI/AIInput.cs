using UnityEngine;

public sealed class AIInput : IInputProvider
{
    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedGrab { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }
    public bool HasBufferedHand3 { get; private set; }
    public bool HasBufferedHand4 { get; private set; }
    public bool HasBufferedParry { get; private set; }
    public bool HasBufferedShield { get; private set; }
    public bool HasBufferedEvade { get; private set; }
    public bool HasBufferedDash { get; private set; }
    public bool IsShieldHeld { get; private set; }
    public bool HasBufferedHeavyAttack { get; private set; }
    public bool IsHeavyAttackHeld { get; private set; }
    public bool WasHeavyAttackReleased { get; private set; }
    public bool WasJumpReleased { get; private set; }

    public void SetDirection(Vector2 direction) => CurrentDirection = direction;
    public void PressJump() => HasBufferedJump = true;
    public void ReleaseJump()
    {
        HasBufferedJump = false;
        WasJumpReleased = true;
    }
    public void PressAttack() => HasBufferedAttack = true;
    public void PressGrab() { }
    public void PressParry() => HasBufferedParry = true;
    public void PressShield() => HasBufferedShield = true;
    public void PressEvade() => HasBufferedEvade = true;
    public void PressDash() => HasBufferedDash = true;
    public void PressCardButton(int index)
    {
        if (index == 0) HasBufferedHand1 = true;
        else if (index == 1) HasBufferedHand2 = true;
        else if (index == 2) HasBufferedHand3 = true;
        else if (index == 3) HasBufferedHand4 = true;
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeJumpRelease() => WasJumpReleased = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeGrab() => HasBufferedGrab = false;
    public void ConsumeParry() => HasBufferedParry = false;
    public void ConsumeShield() => HasBufferedShield = false;
    public void ConsumeEvade() => HasBufferedEvade = false;
    public void ConsumeDash() => HasBufferedDash = false;
    public void ConsumeHeavyAttack() => HasBufferedHeavyAttack = false;
    public void ConsumeHeavyAttackRelease() => WasHeavyAttackReleased = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;
    public void ConsumeHand3() => HasBufferedHand3 = false;
    public void ConsumeHand4() => HasBufferedHand4 = false;

    public void SetShieldHeld(bool held)
    {
        IsShieldHeld = held;
        if (held)
            HasBufferedShield = true;
    }

    public void SetHeavyAttackHeld(bool held)
    {
        if (held == IsHeavyAttackHeld)
            return;

        IsHeavyAttackHeld = held;
        if (held)
        {
            HasBufferedHeavyAttack = true;
            WasHeavyAttackReleased = false;
        }
        else
        {
            WasHeavyAttackReleased = true;
        }
    }

    public void ClearAllInputs()
    {
        ConsumeJump();
        ConsumeJumpRelease();
        ConsumeAttack();
        ConsumeGrab();
        ConsumeParry();
        ConsumeShield();
        ConsumeEvade();
        ConsumeDash();
        ConsumeHeavyAttack();
        ConsumeHeavyAttackRelease();
        ConsumeHand1();
        ConsumeHand2();
        ConsumeHand3();
        ConsumeHand4();
        IsShieldHeld = false;
        IsHeavyAttackHeld = false;
        CurrentDirection = Vector2.zero;
    }

    public void ClearAll() => ClearAllInputs();
}

