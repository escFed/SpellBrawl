using UnityEngine;

public class AIContextBuilder
{
    private Transform selfTransform;
    private CharacterCoordinator selfController;
    private EnergyManager selfEnergy;
    private CharacterHealth selfHealth;
    private Rigidbody2D selfBody;
    private AITarget targetTracker;
    private AINavigation navigation;
    private IAICardSelector cardSelector;
    private float attackRange;
    private float cardRange;
    private float heavyChargeTime;

    public AIContextBuilder(Transform selfTransform, CharacterCoordinator selfController, EnergyManager selfEnergy, CharacterHealth selfHealth, AITarget targetTracker, AINavigation navigation, IAICardSelector cardSelector, float attackRange, float cardRange, float heavyChargeTime)
    {
        this.selfTransform = selfTransform;
        this.selfController = selfController;
        this.selfEnergy = selfEnergy;
        this.selfHealth = selfHealth;
        selfBody = selfTransform != null ? selfTransform.GetComponent<Rigidbody2D>() : null;
        this.targetTracker = targetTracker;
        this.navigation = navigation;
        this.cardSelector = cardSelector;
        this.attackRange = attackRange;
        this.cardRange = cardRange;
        this.heavyChargeTime = heavyChargeTime;
    }

    public AIContext Build()
    {
        Vector3 targetPosition = targetTracker.PerceivedTargetPosition;
        float distanceX = Mathf.Abs(targetPosition.x - selfTransform.position.x);
        float distanceY = targetPosition.y - selfTransform.position.y;
        bool nearEdge = navigation.IsNearEdge(selfController, selfTransform);
        CharacterHealth targetHealth = targetTracker.TargetHealth;
        ICharacterState currentState = selfController != null ? selfController.GetCurrentState() : null;
        bool isGrounded = selfController != null && selfController.IsGrounded;
        bool isGroundLocomotion = selfController != null && (currentState == selfController.States.Idle || currentState == selfController.States.Move || currentState == selfController.States.Crouch);
        bool canUseNormalAttack = selfController != null && (currentState == selfController.States.Idle || currentState == selfController.States.Move || currentState == selfController.States.Jump);
        bool canGroundRoll = selfController != null && selfController.Roll != null && selfController.Roll.CanRoll && isGrounded && (isGroundLocomotion || currentState == selfController.States.Jump);
        bool canAirDodge = selfController != null && selfController.Dodge != null && selfController.Dodge.CanDodge && !isGrounded && currentState == selfController.States.Jump;
        bool hasCompleteHeavySet = selfController != null && selfController.stats != null && selfController.stats.forwardHeavyAttack != null && selfController.stats.upHeavyAttack != null && selfController.stats.downHeavyAttack != null;
        float selfVelocityY = selfBody != null ? selfBody.linearVelocity.y : 0f;
        Vector2 selfVelocity = selfBody != null ? selfBody.linearVelocity : Vector2.zero;
        Rigidbody2D targetBody = targetTracker.Target != null ? targetTracker.Target.GetComponent<Rigidbody2D>() : null;
        Vector2 targetVelocity = targetBody != null ? targetBody.linearVelocity : Vector2.zero;
        CharacterStats stats = selfController != null ? selfController.stats : null;
        bool verticalAttack = distanceY > 0.5f || distanceY < -0.2f;
        AttackStats normalStats = null;
        if (stats != null)
        {
            if (isGrounded)
                normalStats = distanceY > 0.5f ? stats.upTiltAttack :
                    distanceY < -0.2f ? stats.dTiltAttack :
                    distanceX > attackRange * 0.5f ? stats.fTiltAttack : stats.jabAttack;
            else
                normalStats = distanceY > 0.5f ? stats.upAirAttack :
                    distanceY < -0.2f ? stats.downAirAttack :
                    distanceX > attackRange * 0.5f ? stats.forwardAirAttack : stats.neutralAirAttack;
        }
        float normalStartup = normalStats != null ? normalStats.startup : 0.1f;
        Vector2 attackVelocity = isGrounded ? Vector2.zero : new Vector2(0f, selfVelocity.y);
        AttackStats heavyStats = stats != null ?
            (distanceY > 0.5f ? stats.upHeavyAttack :
                distanceY < -0.2f ? stats.downHeavyAttack : stats.forwardHeavyAttack) : null;
        float heavyStartup = heavyStats != null ? heavyStats.startup : 0.1f;
        bool predictedNormalHit = AIAttackPredictor.CanHit(selfTransform.position, attackVelocity, targetPosition, targetVelocity, normalStartup, attackRange, verticalAttack ? 1.35f : 0.9f);
        bool predictedHeavyHit = AIAttackPredictor.CanHit(selfTransform.position, attackVelocity, targetPosition, targetVelocity, heavyStartup + heavyChargeTime, attackRange, verticalAttack ? 1.35f : 0.9f);
        bool incomingHazard = AIDangerSource.IsIncoming(selfTransform, selfVelocity);

        return new AIContext(
            distanceX,
            distanceY,
            selfHealth != null ? selfHealth.currentDamage : 0f,
            targetHealth != null ? targetHealth.currentDamage : 0f,
            selfEnergy != null ? selfEnergy.currentEnergy : 0f,
            distanceX <= attackRange && Mathf.Abs(distanceY) < 1f,
            distanceX <= cardRange,
            distanceY > navigation.VerticalJumpThreshold,
            distanceY < -navigation.VerticalJumpThreshold,
            cardSelector.HasEmptyHand(),
            selfHealth != null && selfHealth.currentDamage >= 85f,
            nearEdge,
            navigation.ShouldRecover(selfController, selfTransform),
            selfController != null && selfController.IsParrying,
            selfController != null && selfController.CanJump,
            isGrounded,
            isGroundLocomotion || currentState == selfController?.States.Jump,
            canUseNormalAttack,
            isGroundLocomotion && selfController.Dash != null && selfController.Dash.CanDash,
            canGroundRoll || canAirDodge,
            isGroundLocomotion && selfController.Shield != null && selfController.Shield.CanActivate,
            selfController != null &&
                (currentState == selfController.States.Idle || currentState == selfController.States.Move),
            isGrounded && selfController != null &&
                (currentState == selfController.States.Idle || currentState == selfController.States.Move),
            isGroundLocomotion && hasCompleteHeavySet,
            isGroundLocomotion,
            !isGrounded && currentState == selfController?.States.Jump &&
                selfVelocityY < 0f && selfController != null && !selfController.Movement.IsFastFalling,
            selfController != null && selfController.cardsEnabled &&
                (currentState == selfController.States.Idle || currentState == selfController.States.Move),
            selfController != null && selfController.Grab != null && selfController.Grab.HasGrabbedTarget,
            IsTargetThreatening(targetTracker.TargetController),
            targetTracker.TargetController != null && targetTracker.TargetController.Shield != null &&
                targetTracker.TargetController.Shield.IsActive,
            selfVelocityY,
            navigation.CanChaseSafely,
            navigation.CanFleeSafely,
            navigation.CanDashSafely,
            navigation.CanEvadeSafely,
            navigation.HasPlannedJump,
            navigation.CanShortHopSafely,
            navigation.CanFastFallSafely,
            predictedNormalHit,
            predictedHeavyHit,
            incomingHazard);
    }

    private static bool IsTargetThreatening(CharacterCoordinator target)
    {
        if (target == null || target.States == null)
            return false;

        ICharacterState state = target.GetCurrentState();
        return state is AttackState || state is GrabState ||
            state == target.States.HeavyCharge || state == target.States.Dash;
    }
}

