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
        hitBox.CloseAllHeavyHitboxes();
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

    public HeavyAttackType ResolveHeavyAttackType()
    {
        Vector2 dir = Input != null ? Input.CurrentDirection : Vector2.zero;
        bool hasHorizontal = Mathf.Abs(dir.x) >= controller.stats.tiltThreshold;
        bool hasUp = dir.y >= controller.stats.tiltThreshold;
        bool hasDown = dir.y <= -controller.stats.tiltThreshold;

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x)))
            return HeavyAttackType.Up;

        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x)))
            return HeavyAttackType.Down;

        return HeavyAttackType.Forward;
    }

    public HeavyAttackStats GetHeavyAttackStats(HeavyAttackType type)
    {
        return type switch
        {
            HeavyAttackType.Up => controller.stats.upHeavyAttack,
            HeavyAttackType.Down => controller.stats.downHeavyAttack,
            _ => controller.stats.forwardHeavyAttack
        };
    }

    public void SetupHeavyAttack(HeavyAttackType type, HeavyAttackStats stats, float chargeRatio)
    {
        float damage = HeavyAttackCharge.CalculateDamage(stats.minDamage, stats.maxDamage, chargeRatio);
        float knockbackMagnitude = HeavyAttackCharge.CalculateKnockback(stats.minKnockback, stats.maxKnockback, chargeRatio);
        Vector2 direction = stats.knockbackDirection.sqrMagnitude > 0.0001f
            ? stats.knockbackDirection.normalized
            : Vector2.right;

        hitBox.SetupHeavyAttack(type, stats, Mathf.RoundToInt(damage), direction * knockbackMagnitude);
    }

    public void OpenHeavyHitbox(HeavyAttackType type) => hitBox.SetHeavyHitbox(type, true);
    public void CloseHeavyHitbox(HeavyAttackType type) => hitBox.SetHeavyHitbox(type, false);
    public void CloseAllHeavyHitboxes() => hitBox.CloseAllHeavyHitboxes();

    public void CheckAndFlip(float directionX) => hitBox.CheckAndFlip(directionX);
    public void FaceDirection(float directionX) => hitBox.FaceDirection(directionX);

    public void SetupJab(GroundAttackStats stats) => hitBox.SetupJab(stats);
    public void SetupFTilt(GroundAttackStats stats) => hitBox.SetupFTilt(stats);
    public void SetupUTilt(GroundAttackStats stats) => hitBox.SetupUTilt(stats);
    public void SetupDTilt(GroundAttackStats stats) => hitBox.SetupDTilt(stats);
    public void SetupNeutralAir(AerialAttackStats stats) => hitBox.SetupNeutralAir(stats);
    public void SetupForwardAir(AerialAttackStats stats) => hitBox.SetupForwardAir(stats);
    public void SetupUpAir(AerialAttackStats stats) => hitBox.SetupUpAir(stats);
    public void SetupDownAir(AerialAttackStats stats) => hitBox.SetupDownAir(stats);
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
