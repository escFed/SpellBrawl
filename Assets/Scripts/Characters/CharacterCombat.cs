using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackSpeedMultiplier = 1f;

    private PlayerController controller;
    private CharacterHitBox hitBox;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        hitBox = GetComponent<CharacterHitBox>();
    }

    private IInputProvider Input => controller.ActiveInput;

    public void TakeHit(float stunDuration)
    {
        controller.Grab?.ReleaseGrabbedTarget();
        controller.stunTimer = stunDuration;
        Input?.ClearAllInputs();
        controller.ChangeState(StateCharacter.Idle);
    }

    public StateCharacter ResolveAttackState()
    {
        Vector2 dir = Input != null ? Input.CurrentDirection : Vector2.zero;
        bool hasHorizontal = Mathf.Abs(dir.x) >= controller.stats.tiltThreshold;
        bool hasUp = dir.y >= controller.stats.tiltThreshold;
        bool hasDown = dir.y <= -controller.stats.tiltThreshold;

        Input?.ConsumeAttack();

        if (!controller.IsGrounded)
        {
            if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return StateCharacter.UpAir;
            if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return StateCharacter.DownAir;
            if (hasHorizontal) return StateCharacter.ForwardAir;

            return StateCharacter.NeutralAir;
        }

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return StateCharacter.UpTilt;
        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return StateCharacter.DownTilt;
        if (hasHorizontal) return StateCharacter.ForwardTilt;

        return StateCharacter.Jab;
    }

    public void CheckAndFlip(float directionX) => hitBox.CheckAndFlip(directionX);
    public void FaceDirection(float directionX) => hitBox.FaceDirection(directionX);

    public void SetupJab(AttackStats stats) => hitBox.SetupJab(stats);
    public void SetupFTilt(AttackStats stats) => hitBox.SetupFTilt(stats);
    public void SetupUTilt(AttackStats stats) => hitBox.SetupUTilt(stats);
    public void SetupDTilt(AttackStats stats) => hitBox.SetupDTilt(stats);
    public void SetupNeutralAir(AttackStats stats) => hitBox.SetupNeutralAir(stats);
    public void SetupForwardAir(AttackStats stats) => hitBox.SetupForwardAir(stats);
    public void SetupUpAir(AttackStats stats) => hitBox.SetupUpAir(stats);
    public void SetupDownAir(AttackStats stats) => hitBox.SetupDownAir(stats);
    public void OpenJabHitbox() => hitBox.SetJabHitbox(true);
    public void CloseJabHitbox() => hitBox.SetJabHitbox(false);
    public void OpenFTiltHitbox() => hitBox.SetFTiltHitbox(true);
    public void CloseFTiltHitbox() => hitBox.SetFTiltHitbox(false);
    public void OpenUTiltHitbox() => hitBox.SetUTiltHitbox(true);
    public void CloseUTiltHitbox() => hitBox.SetUTiltHitbox(false);
    public void OpenDTiltHitbox() => hitBox.SetDTiltHitbox(true);
    public void CloseDTiltHitbox() => hitBox.SetDTiltHitbox(false);
    public void OpenNeutralAirHitbox() => hitBox.SetNeutralAirHitbox(true);
    public void CloseNeutralAirHitbox() => hitBox.SetNeutralAirHitbox(false);
    public void OpenForwardAirHitbox() => hitBox.SetForwardAirHitbox(true);
    public void CloseForwardAirHitbox() => hitBox.SetForwardAirHitbox(false);
    public void OpenUpAirHitbox() => hitBox.SetUpAirHitbox(true);
    public void CloseUpAirHitbox() => hitBox.SetUpAirHitbox(false);
    public void OpenDownAirHitbox() => hitBox.SetDownAirHitbox(true);
    public void CloseDownAirHitbox() => hitBox.SetDownAirHitbox(false);
}