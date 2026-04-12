using UnityEngine;

public interface IInputProvider
{
    Vector2 CurrentDirection { get; }
    bool HasBufferedJump { get; }
    bool HasBufferedAttack { get; }
    bool HasBufferedHand1 { get; }
    bool HasBufferedHand2 { get; }

    void ConsumeJump();
    void ConsumeAttack();
    void ConsumeHand1();
    void ConsumeHand2();
    void ClearAllInputs();
}
