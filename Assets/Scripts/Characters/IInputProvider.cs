using UnityEngine;

public interface IInputProvider
{
    Vector2 CurrentDirection { get; }
    bool HasBufferedJump { get; }
    bool HasBufferedAttack { get; }
    bool HasBufferedHand1 { get; }
    bool HasBufferedHand2 { get; }
    bool HasBufferedHand3 { get; }
    bool HasBufferedHand4 { get; }
    bool HasBufferedDrawCards { get; }
    bool HasBufferedParry { get; }
    bool IsShieldHeld { get; }

    void ConsumeJump();
    void ConsumeAttack();
    void ConsumeHand1();
    void ConsumeHand2();
    void ConsumeHand3();
    void ConsumeHand4();
    void ConsumeDrawCards();
    void ConsumeParry();
    void ClearAllInputs();
}
