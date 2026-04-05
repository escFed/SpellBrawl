using UnityEngine;

public interface IInputProvider
{
    Vector2 CurrentDirection { get; }
    bool HasBufferedJump { get; }
    bool HasBufferedAttack { get; }
    bool HasBufferedFire { get; }
    bool HasBufferedThunder { get; }

    void ConsumeJump();
    void ConsumeAttack();
    void ConsumeFire();
    void ConsumeThunder();
    void ClearAllInputs();
}
