using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    private const float AerialComboWindow = 0.9f;
    private const float VisualHitChainWindow = 0.75f;
    private const int MaximumVisualHitChainStep = 2;

    [Header("Combat Settings")]
    public float attackSpeedMultiplier = 1f;

    private CharacterCoordinator controller;
    private CharacterHitBox hitBox;
    private AerialTracker aerialCombo = new AerialTracker();
    private ICombatHitReceiver lastVisualHitTarget;
    private float lastVisualHitTime;
    private int visualHitChainStep;

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        hitBox = GetComponent<CharacterHitBox>();
    }

    private IInputProvider Input => controller.ActiveInput;

    public void TakeHit(float stunDuration, HitReaction reaction)
    {
        ResetAerialCombo();
        ResetVisualHitChain();
        controller.Grab?.ReleaseGrabbedTarget();
        hitBox.CloseAllAttackHitboxes();
        Input?.ClearAllInputs();
        controller.States.HitStun.Apply(stunDuration, reaction);
    }

    public void PrepareForClash()
    {
        ResetAerialCombo();
        ResetVisualHitChain();
        controller.Grab?.ReleaseGrabbedTarget();
        hitBox.CloseAllAttackHitboxes();
        Input?.ClearAllInputs();
        controller.Movement.ResetKnockback();
        controller.Movement.StopAllMovement();
    }

    public void LaunchFromClash(Vector2 velocity, float stunDuration)
    {
        hitBox.CloseAllAttackHitboxes();
        Input?.ClearAllInputs();
        controller.States.HitStun.Apply(stunDuration, HitReaction.StrongHit);
        controller.Movement.ApplyKnockback(velocity);
    }

    private void OnDisable()
    {
        ResetAerialCombo();
        ResetVisualHitChain();
    }

    public int RegisterVisualHit(ICombatHitReceiver target)
    {
        if (target == null)
            return 0;

        float now = Time.unscaledTime;
        visualHitChainStep = ReferenceEquals(lastVisualHitTarget, target) &&
            now - lastVisualHitTime <= VisualHitChainWindow
            ? Mathf.Min(visualHitChainStep + 1, MaximumVisualHitChainStep)
            : 0;
        lastVisualHitTarget = target;
        lastVisualHitTime = now;
        return visualHitChainStep;
    }

    private void ResetVisualHitChain()
    {
        lastVisualHitTarget = null;
        lastVisualHitTime = 0f;
        visualHitChainStep = 0;
    }

    public float GetAerialSuspensionStrength(ICombatHitReceiver target, AerialAttackStats stats,
        out bool targetGrounded)
    {
        targetGrounded = IsTargetGrounded(target);
        if (target == null || stats == null)
            return 0f;

        return aerialCombo.Preview(target, Time.time, targetGrounded,
            stats.maximumSuspensionHits, stats.suspensionDecay);
    }

    public void BeginAerialCombo(ICombatHitReceiver target)
    {
        if (target != null)
            aerialCombo.Begin(target, Time.time, AerialComboWindow);
    }

    public void RegisterAerialHit(ICombatHitReceiver target, bool targetWasGrounded)
    {
        if (target != null)
            aerialCombo.Confirm(target, Time.time, targetWasGrounded, AerialComboWindow);
    }

    public void ResetAerialCombo() => aerialCombo.Reset();

    private static bool IsTargetGrounded(ICombatHitReceiver target)
    {
        if (target is CharacterHealth health)
        {
            CharacterCoordinator targetController = health.GetComponent<CharacterCoordinator>();
            return targetController != null && targetController.Movement.HasStableGroundContact;
        }

        return target is Dummy dummy && dummy.HasStableGroundContact;
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
            if (AerialInput.IsDownDiagonal(dir) && controller.stats.diagonalDownAirAttack != null && hitBox.HasDiagonalDownAirHitbox)
                return controller.States.DiagonalDownAir;
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

    public static bool IsDiagonalDownAirInput(Vector2 direction)
    {
        return AerialInput.IsDownDiagonal(direction);
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
    public void LockFacing() => hitBox.LockFacing();
    public void UnlockFacing() => hitBox.UnlockFacing();

    public void SetupJab(GroundAttackStats stats) => hitBox.SetupJab(stats);
    public void SetupJabStage(GroundAttackStats stats, int stage) => hitBox.SetupJabStage(stats, stage);
    public void SetupFTilt(GroundAttackStats stats) => hitBox.SetupFTilt(stats);
    public void SetupForwardTiltStage(GroundAttackStats stats, int stage) => hitBox.SetupForwardTiltStage(stats, stage);
    public float FacingSign => hitBox.IsFacingRight ? 1f : -1f;
    public void RegisterForwardTiltHit(AttackHitbox source, AttackStats attackStats)
    {
        if (source == null || !hitBox.IsForwardTiltHitbox(source))
            return;

        if (attackStats == controller.stats.fTiltAttack &&
            controller.GetCurrentState() == controller.States.ForwardTilt)
            controller.States.ForwardTilt.ConfirmHit();
        else if (controller.GetCurrentState() == controller.States.DashAttack)
            controller.States.DashAttack.ConfirmHit(attackStats);
    }
    public void SetupUTilt(GroundAttackStats stats) => hitBox.SetupUTilt(stats);
    public void SetupDTilt(GroundAttackStats stats) => hitBox.SetupDTilt(stats);
    public void SetupNeutralAir(AerialAttackStats stats) => hitBox.SetupNeutralAir(stats);
    public void SetupForwardAir(AerialAttackStats stats) => hitBox.SetupForwardAir(stats);
    public void SetupUpAir(AerialAttackStats stats) => hitBox.SetupUpAir(stats);
    public void SetupDownAir(AerialAttackStats stats) => hitBox.SetupDownAir(stats);
    public void SetupDiagonalDownAir(AerialAttackStats stats) => hitBox.SetupDiagonalDownAir(stats);
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
            case NormalAttackType.DiagonalDownAir: SetupDiagonalDownAir((AerialAttackStats)stats); break;
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
            case NormalAttackType.DiagonalDownAir: hitBox.SetDiagonalDownAirHitbox(active); break;
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
