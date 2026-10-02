public struct AIContext
{
    public float DistanceX { get; }
    public float DistanceY { get; }
    public float SelfDamage { get; }
    public float TargetDamage { get; }
    public float Energy { get; }
    public bool TargetInAttackRange { get; }
    public bool TargetInCardRange { get; }
    public bool TargetAbove { get; }
    public bool TargetBelow { get; }
    public bool EmptyHand { get; }
    public bool InDanger { get; }
    public bool NearEdge { get; }
    public bool ShouldRecover { get; }
    public bool IsParrying { get; }
    public bool CanJump { get; }
    public bool IsGrounded { get; }
    public bool CanMove { get; }
    public bool CanAttack { get; }
    public bool CanDash { get; }
    public bool CanEvade { get; }
    public bool CanShield { get; }
    public bool CanParry { get; }
    public bool CanGrab { get; }
    public bool CanHeavyAttack { get; }
    public bool CanCrouch { get; }
    public bool CanFastFall { get; }
    public bool CanUseCards { get; }
    public bool HasGrabbedTarget { get; }
    public bool TargetThreatening { get; }
    public bool TargetShielding { get; }
    public float SelfVelocityY { get; }
    public bool CanChaseSafely { get; }
    public bool CanFleeSafely { get; }
    public bool CanDashSafely { get; }
    public bool CanEvadeSafely { get; }
    public bool HasPlannedJump { get; }
    public bool CanShortHopSafely { get; }
    public bool CanFastFallSafely { get; }
    public bool PredictedAttackHit { get; }
    public bool PredictedHeavyHit { get; }
    public bool IncomingHazard { get; }

    public AIContext(
        float distanceX,
        float distanceY,
        float selfDamage,
        float targetDamage,
        float energy,
        bool targetInAttackRange,
        bool targetInCardRange,
        bool targetAbove,
        bool targetBelow,
        bool emptyHand,
        bool inDanger,
        bool nearEdge,
        bool shouldRecover,
        bool isParrying,
        bool canJump,
        bool isGrounded = false,
        bool canMove = false,
        bool canAttack = false,
        bool canDash = false,
        bool canEvade = false,
        bool canShield = false,
        bool canParry = false,
        bool canGrab = false,
        bool canHeavyAttack = false,
        bool canCrouch = false,
        bool canFastFall = false,
        bool canUseCards = false,
        bool hasGrabbedTarget = false,
        bool targetThreatening = false,
        bool targetShielding = false,
        float selfVelocityY = 0f,
        bool canChaseSafely = true,
        bool canFleeSafely = true,
        bool canDashSafely = true,
        bool canEvadeSafely = true,
        bool hasPlannedJump = true,
        bool canShortHopSafely = true,
        bool canFastFallSafely = true,
        bool? predictedAttackHit = null,
        bool? predictedHeavyHit = null,
        bool incomingHazard = false)
    {
        DistanceX = distanceX;
        DistanceY = distanceY;
        SelfDamage = selfDamage;
        TargetDamage = targetDamage;
        Energy = energy;
        TargetInAttackRange = targetInAttackRange;
        TargetInCardRange = targetInCardRange;
        TargetAbove = targetAbove;
        TargetBelow = targetBelow;
        EmptyHand = emptyHand;
        InDanger = inDanger;
        NearEdge = nearEdge;
        ShouldRecover = shouldRecover;
        IsParrying = isParrying;
        CanJump = canJump;
        IsGrounded = isGrounded;
        CanMove = canMove;
        CanAttack = canAttack;
        CanDash = canDash;
        CanEvade = canEvade;
        CanShield = canShield;
        CanParry = canParry;
        CanGrab = canGrab;
        CanHeavyAttack = canHeavyAttack;
        CanCrouch = canCrouch;
        CanFastFall = canFastFall;
        CanUseCards = canUseCards;
        HasGrabbedTarget = hasGrabbedTarget;
        TargetThreatening = targetThreatening;
        TargetShielding = targetShielding;
        SelfVelocityY = selfVelocityY;
        CanChaseSafely = canChaseSafely;
        CanFleeSafely = canFleeSafely;
        CanDashSafely = canDashSafely;
        CanEvadeSafely = canEvadeSafely;
        HasPlannedJump = hasPlannedJump;
        CanShortHopSafely = canShortHopSafely;
        CanFastFallSafely = canFastFallSafely;
        PredictedAttackHit = predictedAttackHit ?? targetInAttackRange;
        PredictedHeavyHit = predictedHeavyHit ?? targetInAttackRange;
        IncomingHazard = incomingHazard;
    }
}
