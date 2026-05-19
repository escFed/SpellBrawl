using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackSpeedMultiplier = 1f;
    public Transform throwPoint;

    private PlayerController controller;
    private CharacterHitBox hitBox;
    private IInputProvider input;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        hitBox = GetComponent<CharacterHitBox>();
        input = GetComponent<IInputProvider>();
    }

    public void TakeHit(float stunDuration)
    {
        controller.stunTimer = stunDuration;
        input.ClearAllInputs();
        controller.ChangeState(StateCharacter.Idle);
    }

    public StateCharacter ResolveAttackState()
    {
        Vector2 dir = input.CurrentDirection;
        bool hasHorizontal = Mathf.Abs(dir.x) >= controller.stats.tiltThreshold;
        bool hasUp = dir.y >= controller.stats.tiltThreshold;
        bool hasDown = dir.y <= -controller.stats.tiltThreshold;

        input.ConsumeAttack();

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return StateCharacter.UpTilt;
        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return StateCharacter.DownTilt;
        if (hasHorizontal) return StateCharacter.ForwardTilt;

        return StateCharacter.Jab;
    }

    public void CheckAndFlip(float directionX) => hitBox.CheckAndFlip(directionX);

    public void SetupJab(AttackStats stats) => hitBox.SetupJab(stats);
    public void SetupFTilt(AttackStats stats) => hitBox.SetupFTilt(stats);
    public void SetupUTilt(AttackStats stats) => hitBox.SetupUTilt(stats);
    public void SetupDTilt(AttackStats stats) => hitBox.SetupDTilt(stats);
    public void OpenJabHitbox() => hitBox.SetJabHitbox(true);
    public void CloseJabHitbox() => hitBox.SetJabHitbox(false);
    public void OpenFTiltHitbox() => hitBox.SetFTiltHitbox(true);
    public void CloseFTiltHitbox() => hitBox.SetFTiltHitbox(false);
    public void OpenUTiltHitbox() => hitBox.SetUTiltHitbox(true);
    public void CloseUTiltHitbox() => hitBox.SetUTiltHitbox(false);
    public void OpenDTiltHitbox() => hitBox.SetDTiltHitbox(true);
    public void CloseDTiltHitbox() => hitBox.SetDTiltHitbox(false);
}
