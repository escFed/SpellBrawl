using UnityEngine;

public class AIInput
{
    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }
    public bool HasBufferedHand3 { get; private set; }
    public bool HasBufferedHand4 { get; private set; }
    public bool HasBufferedHand5 { get; private set; }
    public bool HasBufferedParry { get; private set; }
    public bool HasBufferedDrawCards { get; private set; }

    public void SetDirection(Vector2 direction) => CurrentDirection = direction;
    public void PressJump() => HasBufferedJump = true;
    public void PressAttack() => HasBufferedAttack = true;
    public void PressParry() => HasBufferedParry = true;
    public void PressDrawCards() => HasBufferedDrawCards = true;

    public void PressCardButton(int index)
    {
        if (index == 0) HasBufferedHand1 = true;
        else if (index == 1) HasBufferedHand2 = true;
        else if (index == 2) HasBufferedHand3 = true;
        else if (index == 3) HasBufferedHand4 = true;
        else if (index == 4) HasBufferedHand5 = true;
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeParry() => HasBufferedParry = false;
    public void ConsumeDrawCards() => HasBufferedDrawCards = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;
    public void ConsumeHand3() => HasBufferedHand3 = false;
    public void ConsumeHand4() => HasBufferedHand4 = false;
    public void ConsumeHand5() => HasBufferedHand5 = false;

    public void ClearAll()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeParry();
        ConsumeDrawCards();
        ConsumeHand1();
        ConsumeHand2();
        ConsumeHand3();
        ConsumeHand4();
        ConsumeHand5();
        CurrentDirection = Vector2.zero;
    }
}

