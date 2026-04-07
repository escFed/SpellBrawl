using UnityEngine;

public interface IInputProvider
{
    Vector2 CurrentDirection { get; }
    bool HasBufferedJump { get; }
    bool HasBufferedAttack { get; }
    bool HasBufferedSpecial { get; }


    void ConsumeJump();
    void ConsumeAttack();
    void ConsumeSpecial();
    void ClearAllInputs();
}
