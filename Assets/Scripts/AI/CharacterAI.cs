using UnityEngine;

[RequireComponent(typeof(CharacterCoordinator))]
public sealed class CharacterAI : MonoBehaviour, IInputProvider, IDirectionalInfluenceProvider
{
    private const int CurrentProfileVersion = 1;

    [Header("AI Profile")]
    public AIProfile profile = new AIProfile();
    [SerializeField, HideInInspector]
    private int profileVersion;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float cardRange = 6f;
    public float idealSpacing = 1.5f;

    [Header("Navigation Settings")]
    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 5f;
    public float verticalJumpThreshold = 1.5f;
    public float recoveryHeightThreshold = -3f;

    public CharacterCoordinator SelfController { get; private set; }
    public CharacterHealth SelfHealth { get; private set; }
    public CharacterDeck SelfDeck { get; private set; }
    public Transform Target => targetTracker?.Target;

    public AIDecision currentDecision = AIDecision.Idle;

    private readonly AIInput input = new AIInput();
    private AITarget targetTracker;
    private AINavigation navigation;
    private AICardSelector cardSelector;
    private AIContextBuilder contextBuilder;
    private AIUtility utility;
    private AIExecutor executor;
    private readonly AIActionMemory memory = new AIActionMemory();
    private Rigidbody2D selfBody;
    private bool hazardSeen;
    private float nextHazardCheck;
    private float thinkTimer;

    public bool IsDebugFrozen { get; private set; }

    public void SetDebugFrozen(bool frozen)
    {
        IsDebugFrozen = frozen;
        ResetExecution();
        thinkTimer = 0f;
        currentDecision = AIDecision.Idle;
    }


    public Vector2 CurrentDirection => input.CurrentDirection;
    public bool HasBufferedJump => input.HasBufferedJump;
    public bool WasJumpReleased => input.WasJumpReleased;
    public bool HasBufferedAttack => input.HasBufferedAttack;
    public bool HasBufferedGrab => input.HasBufferedGrab;
    public bool HasBufferedHand1 => input.HasBufferedHand1;
    public bool HasBufferedHand2 => input.HasBufferedHand2;
    public bool HasBufferedHand3 => input.HasBufferedHand3;
    public bool HasBufferedHand4 => input.HasBufferedHand4;
    public bool HasBufferedParry => input.HasBufferedParry;
    public bool HasBufferedShield => input.HasBufferedShield;
    public bool HasBufferedEvade => input.HasBufferedEvade;
    public bool HasBufferedDash => input.HasBufferedDash;
    public bool IsShieldHeld => input.IsShieldHeld;
    public bool HasBufferedDrawCards => input.HasBufferedDrawCards;
    public bool HasBufferedHeavyAttack => input.HasBufferedHeavyAttack;
    public bool IsHeavyAttackHeld => input.IsHeavyAttackHeld;
    public bool WasHeavyAttackReleased => input.WasHeavyAttackReleased;
    public Vector2 DirectionalInfluence
    {
        get
        {
            if (targetTracker != null && targetTracker.Target != null)
            {
                Vector2 towardOpponent = targetTracker.Target.position - transform.position;
                if (towardOpponent.sqrMagnitude > 0.01f)
                    return towardOpponent.normalized;
            }

            return input.CurrentDirection;
        }
    }

    private void Awake()
    {
        SelfController = GetComponent<CharacterCoordinator>();
        SelfHealth = GetComponent<CharacterHealth>();
        SelfDeck = GetComponent<CharacterDeck>();
        selfBody = GetComponent<Rigidbody2D>();

        UpgradeProfile();

        targetTracker = new AITarget();
        targetTracker.Initialize(SelfController, transform.position);

        navigation = new AINavigation();
        navigation.Configure(
            groundLayer,
            lookAheadDistance,
            fallCheckDepth,
            verticalJumpThreshold,
            recoveryHeightThreshold);

        cardSelector = new AICardSelector();
        cardSelector.Initialize(SelfController, SelfDeck, targetTracker);

        contextBuilder = new AIContextBuilder(
            transform,
            SelfController,
            SelfHealth,
            targetTracker,
            navigation,
            cardSelector,
            attackRange,
            cardRange,
            profile.heavyChargeTime);

        utility = new AIUtility(new System.Random(GetInstanceID()));
        executor = new AIExecutor();
    }

    private void OnEnable()
    {
        thinkTimer = 0f;
        executor?.Reset(input);
        navigation?.Reset();
        memory.Reset();
        hazardSeen = false;
        nextHazardCheck = 0f;
    }

    private void OnDisable()
    {
        if (executor != null)
            executor.Reset(input);
        else
            input.ClearAllInputs();
        navigation?.Reset();
        memory.Reset();
        currentDecision = AIDecision.Idle;
    }

    private void OnValidate()
    {
        UpgradeProfile();
        attackRange = Mathf.Max(0f, attackRange);
        cardRange = Mathf.Max(0f, cardRange);
        idealSpacing = Mathf.Max(0f, idealSpacing);
        lookAheadDistance = Mathf.Max(0f, lookAheadDistance);
        fallCheckDepth = Mathf.Max(0f, fallCheckDepth);
        verticalJumpThreshold = Mathf.Max(0f, verticalJumpThreshold);
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (IsDebugFrozen)
            return;
#endif
        if (SelfController == null || SelfController.IsDead || SelfController.IsHitStunned)
        {
            ResetExecution();
            return;
        }

        if (CombatFeedback.IsHitStopActive)
            return;

        targetTracker.Tick();
        if (targetTracker.Target == null)
        {
            ResetExecution();
            currentDecision = AIDecision.Idle;
            return;
        }

        if (targetTracker.TargetHealth != null)
            memory.Observe(targetTracker.TargetHealth.currentDamage, Time.time);

        if (Time.time >= nextHazardCheck)
        {
            nextHazardCheck = Time.time + 0.08f;
            bool hazard = AIDangerSource.IsIncoming(transform,
                selfBody != null ? selfBody.linearVelocity : Vector2.zero);
            if (hazard && !hazardSeen)
                thinkTimer = 0f;
            hazardSeen = hazard;
        }

        executor.Tick(input, Time.time);
        navigation.GuardInput(input, currentDecision);
        thinkTimer -= Time.deltaTime;
        if (navigation.HasCommittedJump)
            return;
        if (IsWaitingForCommittedAction())
            return;

        if (!CanChooseDecision())
            return;

        if (thinkTimer > 0f)
            return;

        thinkTimer = profile.reactionTime;
        targetTracker.UpdatePerception();
        navigation.Refresh(SelfController, transform, targetTracker.PerceivedTargetPosition);

        AIContext context = contextBuilder.Build();
        AIActionScore action = utility.ChooseDecision(
            context,
            profile,
            cardSelector,
            currentDecision,
            attackRange,
            idealSpacing,
            memory,
            Time.time);

        currentDecision = action.Decision;
        executor.Execute(
            action,
            input,
            SelfController,
            transform,
            navigation,
            targetTracker.PerceivedTargetPosition,
            attackRange,
            profile,
            Time.time);
        memory.Record(action,
            targetTracker.TargetHealth != null ? targetTracker.TargetHealth.currentDamage : 0,
            Time.time);
    }

    public void ConsumeJump() => input.ConsumeJump();
    public void ConsumeJumpRelease() => input.ConsumeJumpRelease();
    public void ConsumeAttack() => input.ConsumeAttack();
    public void ConsumeGrab() => input.ConsumeGrab();
    public void ConsumeParry() => input.ConsumeParry();
    public void ConsumeShield() => input.ConsumeShield();
    public void ConsumeEvade() => input.ConsumeEvade();
    public void ConsumeDash() => input.ConsumeDash();
    public void ConsumeHeavyAttack() => input.ConsumeHeavyAttack();
    public void ConsumeHeavyAttackRelease() => input.ConsumeHeavyAttackRelease();
    public void ConsumeDrawCards() => input.ConsumeDrawCards();
    public void ConsumeHand1() => input.ConsumeHand1();
    public void ConsumeHand2() => input.ConsumeHand2();
    public void ConsumeHand3() => input.ConsumeHand3();
    public void ConsumeHand4() => input.ConsumeHand4();
    public void SetHeavyAttackHeld(bool held) => input.SetHeavyAttackHeld(held);
    public void ClearAllInputs() => ResetExecution();

    private bool IsWaitingForCommittedAction()
    {
        ICharacterState state = SelfController.GetCurrentState();
        return state == SelfController.States.HeavyCharge ||
            state == SelfController.States.Shield;
    }

    private bool CanChooseDecision()
    {
        ICharacterState state = SelfController.GetCurrentState();
        return state == SelfController.States.Idle ||
            state == SelfController.States.Move ||
            state == SelfController.States.Crouch ||
            state == SelfController.States.Jump;
    }

    private void ResetExecution()
    {
        if (executor != null)
            executor.Reset(input);
        else
            input.ClearAllInputs();
        navigation?.Reset();
    }

    private void UpgradeProfile()
    {
        profile ??= new AIProfile();

        if (profileVersion < 1)
        {
            profile.shieldUsage = 0.55f;
            profile.evadeSkill = 0.6f;
            profile.dashUsage = 0.55f;
            profile.heavyAttackUsage = 0.5f;
            profile.boostUsage = 0.65f;
            profile.shortHopHoldTime = 0.08f;
            profile.heavyChargeTime = 0.65f;
            profile.shieldHoldTime = 0.75f;
            profileVersion = CurrentProfileVersion;
        }

        profile.Sanitize();
    }
}
