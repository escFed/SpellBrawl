using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackSpeedMultiplier = 1f;

    private CharacterCoordinator controller;
    private CharacterHitBox hitBox;

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        hitBox = GetComponent<CharacterHitBox>();
    }

    private IInputProvider Input => controller.ActiveInput;

    public void TakeHit(float stunDuration, HitReaction reaction)
    {
        controller.Grab?.ReleaseGrabbedTarget();
        hitBox.CloseAllHeavyHitboxes();
        Input?.ClearAllInputs();
        controller.States.HitStun.Apply(stunDuration, reaction);
    }

    public ICharacterState ResolveAttackState()
    {
        Vector2 dir = Input != null ? Input.CurrentDirection : Vector2.zero;
        bool hasHorizontal = Mathf.Abs(dir.x) >= controller.stats.tiltThreshold;
        bool hasUp = dir.y >= controller.stats.tiltThreshold;
        bool hasDown = dir.y <= -controller.stats.tiltThreshold;

        Input?.ConsumeAttack();

        if (!controller.IsGrounded)
        {
            if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return controller.States.UpAir;
            if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return controller.States.DownAir;
            if (hasHorizontal) return controller.States.ForwardAir;

            return controller.States.NeutralAir;
        }

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return controller.States.UpTilt;
        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return controller.States.DownTilt;
        if (hasHorizontal) return controller.States.ForwardTilt;

        return controller.States.Jab;
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
        float hitStun = HeavyAttackCharge.CalculateHitStun(stats.hitStun, stats.maxHitStun, chargeRatio);
        Vector2 direction = stats.knockbackDirection.sqrMagnitude > 0.0001f ? stats.knockbackDirection.normalized : Vector2.right;

        float growth = Mathf.Lerp(stats.launch?.growth ?? 3f, stats.maxKnockbackGrowth, Mathf.Clamp01(chargeRatio));
        hitBox.SetupHeavyAttack(type, stats, Mathf.RoundToInt(damage), direction * knockbackMagnitude, hitStun, growth);
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
    public void SetupNormalAttack(NormalAttackType attackType, NormalAttackStats stats)
    {
        switch (attackType)
        {
            case NormalAttackType.Jab: SetupJab((GroundAttackStats)stats); break;
            case NormalAttackType.ForwardTilt: SetupFTilt((GroundAttackStats)stats); break;
            case NormalAttackType.UpTilt: SetupUTilt((GroundAttackStats)stats); break;
            case NormalAttackType.DownTilt: SetupDTilt((GroundAttackStats)stats); break;
            case NormalAttackType.NeutralAir: SetupNeutralAir((AerialAttackStats)stats); break;
            case NormalAttackType.ForwardAir: SetupForwardAir((AerialAttackStats)stats); break;
            case NormalAttackType.UpAir: SetupUpAir((AerialAttackStats)stats); break;
            case NormalAttackType.DownAir: SetupDownAir((AerialAttackStats)stats); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(attackType), attackType, "Not a normal attack.");
        }
    }

    public void SetNormalAttackHitbox(NormalAttackType attackType, bool active)
    {
        switch (attackType)
        {
            case NormalAttackType.Jab: hitBox.SetJabHitbox(active); break;
            case NormalAttackType.ForwardTilt: hitBox.SetFTiltHitbox(active); break;
            case NormalAttackType.UpTilt: hitBox.SetUTiltHitbox(active); break;
            case NormalAttackType.DownTilt: hitBox.SetDTiltHitbox(active); break;
            case NormalAttackType.NeutralAir: hitBox.SetNeutralAirHitbox(active); break;
            case NormalAttackType.ForwardAir: hitBox.SetForwardAirHitbox(active); break;
            case NormalAttackType.UpAir: hitBox.SetUpAirHitbox(active); break;
            case NormalAttackType.DownAir: hitBox.SetDownAirHitbox(active); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(attackType), attackType, "Not a normal attack.");
        }
    }
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
