using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterStats stats;

    public bool IsDead { get; private set; }
    public int PlayerIndex { get; set; }
    public bool IsParrying { get; set; }
    public bool IsIntangible { get; set; }
    public bool IsHitStunned => stateMachine != null && stateMachine.CurrentState == stateMachine.HitStun;

    public int JumpsRemaining => airJumpsRemaining + (CanGroundJump ? 1 : 0);
    public bool CanGroundJump => groundJumpAvailable && ((Movement != null && Movement.HasStableGroundContact) || coyoteTimeRemaining > 0f);
    public bool CanJump => CanGroundJump || (!IsGrounded && airJumpsRemaining > 0);
    public float CoyoteTimeRemaining => coyoteTimeRemaining;

    private int airJumpsRemaining;
    private bool groundJumpAvailable;
    private bool wasStablyGrounded;
    private float coyoteTimeRemaining;
    private bool wasPaused;
    public bool controlsEnabled = true;
    public bool cardsEnabled = true;

    private IInputProvider input;
    private CharacterDeck deck;
    private CharacterParry parry;
    private System.Collections.Generic.Dictionary<int, AnimatorControllerParameterType> animatorParameterTypes = new System.Collections.Generic.Dictionary<int, AnimatorControllerParameterType>();

    public Animator Anim { get; private set; }
    public CharacterCombat Combat { get; private set; }
    public CharacterGrab Grab { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterHealth Health { get; private set; }
    public CharacterShield Shield { get; private set; }
    public CharacterRoll Roll { get; private set; }
    public CharacterDodge Dodge { get; private set; }
    public CharacterDash Dash { get; private set; }
    public CharacterHitFeedback HitFeedback { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public CharacterVisuals Visuals { get; private set; }

    public IInputProvider ActiveInput => input;
    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed => input.HasBufferedJump;
    public bool IsGrounded => Movement.IsGrounded;
    public bool AttackInput => input.HasBufferedAttack;
    public bool GrabInput => input.HasBufferedGrab;
    public bool EvadePressed => input.HasBufferedEvade;
    public bool DashPressed => input.HasBufferedDash;
    public bool HeavyAttackPressed => input.HasBufferedHeavyAttack;

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        deck = GetComponent<CharacterDeck>();
        parry = GetComponent<CharacterParry>();
        Combat = GetComponent<CharacterCombat>();
        Grab = GetComponent<CharacterGrab>();
        Movement = GetComponent<CharacterMovement>();
        Health = GetComponent<CharacterHealth>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
        Visuals = GetComponent<CharacterVisuals>();
        Anim = GetComponentInChildren<Animator>();
        CacheAnimatorParameters();
        Shield = GetComponent<CharacterShield>();
        Roll = GetComponent<CharacterRoll>();
        Dodge = GetComponent<CharacterDodge>();
        Dash = GetComponent<CharacterDash>();
        HitFeedback = GetComponent<CharacterHitFeedback>();

        if (Shield == null)
            Shield = gameObject.AddComponent<CharacterShield>();

        if (Roll == null)
            Roll = gameObject.AddComponent<CharacterRoll>();

        if (Dodge == null)
            Dodge = gameObject.AddComponent<CharacterDodge>();

        if (Dash == null)
            Dash = gameObject.AddComponent<CharacterDash>();

        if (HitFeedback == null)
            HitFeedback = gameObject.AddComponent<CharacterHitFeedback>();

        Shield.Initialize(Sprite);
        HitFeedback.Initialize(Sprite);

        stateMachine = new StateMachine();
        stateMachine.Idle = new IdleState(this, stateMachine);
        stateMachine.Move = new MoveState(this, stateMachine);
        stateMachine.Jump = new JumpState(this, stateMachine);
        stateMachine.Crouch = new CrouchState(this, stateMachine);
        stateMachine.Shield = new ShieldState(this, stateMachine);
        stateMachine.Roll = new RollState(this, stateMachine);
        stateMachine.Dodge = new DodgeState(this, stateMachine);
        stateMachine.Dash = new DashState(this, stateMachine);
        stateMachine.Jab = new JabState(this, stateMachine, stats.jabAttack);
        stateMachine.ForwardTilt = new ForwardTiltState(this, stateMachine, stats.fTiltAttack);
        stateMachine.UpTilt = new UpTiltState(this, stateMachine, stats.upTiltAttack);
        stateMachine.DownTilt = new DownTiltState(this, stateMachine, stats.dTiltAttack);
        stateMachine.NeutralAir = new NeutralAirState(this, stateMachine, stats.neutralAirAttack);
        stateMachine.ForwardAir = new ForwardAirState(this, stateMachine, stats.forwardAirAttack);
        stateMachine.UpAir = new UpAirState(this, stateMachine, stats.upAirAttack);
        stateMachine.DownAir = new DownAirState(this, stateMachine, stats.downAirAttack);
        stateMachine.HeavyCharge = new HeavyChargeState(this, stateMachine);
        stateMachine.HeavyAttack = new HeavyAttackState(this, stateMachine);
        stateMachine.DashAttack = new DashAttackState(this, stateMachine, stats.dashAttack != null ? stats.dashAttack : stats.fTiltAttack);
        stateMachine.Grab = new NormalGrabState(this, stateMachine, stats.grabStats);
        stateMachine.PivotGrab = new PivotGrabState(this, stateMachine, stats.pivotGrabStats);
        stateMachine.DashGrab = new DashGrabState(this, stateMachine, stats.dashGrabStats != null ? stats.dashGrabStats : stats.grabStats);
        stateMachine.GrabHold = new GrabHoldState(this, stateMachine);
        stateMachine.Pummel = new PummelState(this, stateMachine);
        stateMachine.Throw = new ThrowState(this, stateMachine);
        stateMachine.Card = new CardState(this, stateMachine);
        stateMachine.Die = new DieState(this, stateMachine);
        stateMachine.Parry = new ParryState(this, stateMachine);
        stateMachine.HitStun = new HitStunState(this, stateMachine);

        if (GetComponent<IGrabbable>() == null)
            gameObject.AddComponent<CharacterGrabbable>();
    }

    private void Start()
    {
        Movement.RefreshGroundedState();
        Visuals.Initialize(Sprite, PlayerIndex);
        stateMachine.ChangeState(StateCharacter.Idle);
        ResetJumps();
        wasStablyGrounded = Movement.HasStableGroundContact;

        bool isAI = (PlayerIndex == 1);

        if (isAI)
        {
            input = GetComponent<CharacterAI>();
        }
        else
        {
            input = GetComponent<CharacterBrain>();
        }

        if (stats != null && stats.characterIcon != null)
        {
            UIEvents.OnIconSet?.Invoke(PlayerIndex, stats.characterIcon);
        }

        Health.UpdateUI();
        GetComponent<EnergyManager>().UpdateUI();
    }

    public IState GetCurrentState() => stateMachine.CurrentState;
    public void ChangeState(StateCharacter newState) => stateMachine.ChangeState(newState);

    private void Update()
    {
        if (input == null)
            return;

        if (IsDead || stateMachine.CurrentState == stateMachine.Die)
        {
            input.ClearAllInputs();
            return;
        }

        if (PauseMenu.isPaused)
        {
            input.ClearAllInputs();
            wasPaused = true;
            return;
        }

        if (wasPaused)
        {
            input.ClearAllInputs();
            wasPaused = false;
        }

        if (!controlsEnabled)
        {
            input.ClearAllInputs();
            return;
        }

        if (CombatFeedback.IsHitStopActive) return;

        Movement.RefreshGroundedState();
        bool landedThisFrame = UpdateJumpAvailability();
        EnterAirborneLocomotionIfNeeded();

        if (TryHandleBufferedLandingJump(landedThisFrame))
            return;

        IState stateBeforeUpdate = stateMachine.CurrentState;

        if (IsHitStunned)
        {
            stateMachine.Update();
            TryExecuteBufferedInputAfterRecovery(stateBeforeUpdate);
            return;
        }

        if (TryHandlePriorityInputs())
            return;

        stateMachine.Update();
        TryExecuteBufferedInputAfterRecovery(stateBeforeUpdate);
    }

    private bool TryHandlePriorityInputs()
    {
        if (input.HasBufferedShield)
            input.ConsumeShield();

        if (input.IsShieldHeld)
        {
            bool canStartShield = Shield.CanActivate && IsGrounded &&
                (stateMachine.CurrentState == stateMachine.Idle ||
                 stateMachine.CurrentState == stateMachine.Move ||
                 stateMachine.CurrentState == stateMachine.Crouch);

            if (canStartShield)
            {
                stateMachine.ChangeState(StateCharacter.Shield);
                return true;
            }
        }

        bool canTurnFromInput = stateMachine.CurrentState != stateMachine.HeavyAttack;
        if (canTurnFromInput)
            Combat.FaceDirection(MoveInput.x);

        if (TryHandleDashInput())
            return true;

        bool isEvading = stateMachine.CurrentState == stateMachine.Roll ||
            stateMachine.CurrentState == stateMachine.Dodge;

        if (!isEvading && input.HasBufferedEvade)
        {
            bool canStartRoll = Roll.CanRoll && IsGrounded &&
                (stateMachine.CurrentState == stateMachine.Idle ||
                 stateMachine.CurrentState == stateMachine.Move ||
                 stateMachine.CurrentState == stateMachine.Crouch ||
                 stateMachine.CurrentState == stateMachine.HeavyCharge ||
                 stateMachine.CurrentState == stateMachine.Jump);

            bool canStartDodge = Dodge.CanDodge && !IsGrounded &&
                stateMachine.CurrentState == stateMachine.Jump;

            if (canStartRoll)
            {
                input.ConsumeEvade();
                stateMachine.ChangeState(StateCharacter.Roll);
                return true;
            }

            if (canStartDodge)
            {
                input.ConsumeEvade();
                stateMachine.ChangeState(StateCharacter.Dodge);
                return true;
            }
        }

        if (TryHandleHeavyAttackInput())
            return true;

        if (input.HasBufferedParry)
        {
            if (parry.TryParry())
            {
                input.ConsumeParry();
                return true;
            }
        }

        bool canUseCards = cardsEnabled && (stateMachine.CurrentState == stateMachine.Idle || stateMachine.CurrentState == stateMachine.Move);

        if (canUseCards && input.HasBufferedDrawCards)
        {
            deck.TryDrawNewHand();
            input.ConsumeDrawCards();
        }

        if (canUseCards)
        {
            if (input.HasBufferedHand1) { deck.TryUseCardFromHand(0); input.ConsumeHand1(); }
            else if (input.HasBufferedHand2) { deck.TryUseCardFromHand(1); input.ConsumeHand2(); }
            else if (input.HasBufferedHand3) { deck.TryUseCardFromHand(2); input.ConsumeHand3(); }
            else if (input.HasBufferedHand4) { deck.TryUseCardFromHand(3); input.ConsumeHand4(); }
        }

        return false;
    }

    private void TryExecuteBufferedInputAfterRecovery(IState previousState)
    {
        // Attack recovery and hitstun can end inside StateMachine.Update. Dispatch once
        // more so a still-buffered command runs on that same newly legal frame.
        if (!WasBufferedRecoveryState(previousState) || previousState == stateMachine.CurrentState ||
            !IsLocomotionState(stateMachine.CurrentState))
            return;

        if (TryHandlePriorityInputs())
            return;

        if (HasBufferedLocomotionInput(stateMachine.CurrentState))
            stateMachine.Update();
    }

    private bool WasBufferedRecoveryState(IState state)
    {
        return state == stateMachine.HitStun || state is AttackState;
    }

    private bool IsLocomotionState(IState state)
    {
        return state == stateMachine.Idle || state == stateMachine.Move ||
            state == stateMachine.Crouch || state == stateMachine.Jump;
    }

    private bool HasBufferedLocomotionInput(IState state)
    {
        if (state == stateMachine.Idle || state == stateMachine.Move)
            return input.HasBufferedGrab || input.HasBufferedAttack ||
                (input.HasBufferedJump && CanJump);

        if (state == stateMachine.Crouch)
            return input.HasBufferedJump && CanJump;

        if (state == stateMachine.Jump)
            return input.HasBufferedAttack ||
                (input.HasBufferedJump && CanJump);

        return false;
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        if (PauseMenu.isPaused) return;
        Movement.StepPhysics();
        if (!controlsEnabled) return;
        stateMachine.FixedUpdate();
    }
    public void ExecuteCardState(ICardable cardToUse)
    {
        stateMachine.Card.SetCard(cardToUse, 0.5f);
        stateMachine.ChangeState(StateCharacter.Card);
    }

    public bool TryPerformJump()
    {
        if (input == null || Movement == null || !CanJump)
            return false;

        if (CanGroundJump)
        {
            groundJumpAvailable = false;
            coyoteTimeRemaining = 0f;
        }
        else
        {
            airJumpsRemaining = Mathf.Max(0, airJumpsRemaining - 1);
        }

        input.ConsumeJump();
        Movement.ApplyJumpForce();
        return true;
    }

    public void HandleAirborneMovementInput()
    {
        if (input == null || Movement == null || IsGrounded)
            return;

        if (input.WasJumpReleased)
        {
            Movement.TryApplyShortHop();
            input.ConsumeJumpRelease();
        }

        Movement.TryStartFastFall(MoveInput.y);
    }

    public void ResetJumps()
    {
        int maxJumps = stats != null ? Mathf.Max(0, stats.maxJumps) : 1;
        airJumpsRemaining = Mathf.Max(0, maxJumps - 1);
        groundJumpAvailable = maxJumps > 0;
        coyoteTimeRemaining = 0f;
    }

    public void CancelGroundJumpAvailability()
    {
        ForfeitGroundJump();
        wasStablyGrounded = false;
    }

    private bool UpdateJumpAvailability()
    {
        bool isStablyGrounded = Movement.HasStableGroundContact;
        bool landedThisFrame = isStablyGrounded && !wasStablyGrounded;

        if (landedThisFrame)
        {
            ResetJumps();
        }
        else if (!isStablyGrounded && wasStablyGrounded && groundJumpAvailable)
        {
            if (CanArmCoyoteTime())
                coyoteTimeRemaining = stats != null ? Mathf.Max(0f, stats.coyoteTime) : 0.1f;
            else
                ForfeitGroundJump();
        }
        else if (!isStablyGrounded && coyoteTimeRemaining > 0f)
        {
            coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - Time.deltaTime);
            if (coyoteTimeRemaining <= 0f)
                groundJumpAvailable = false;
        }

        wasStablyGrounded = isStablyGrounded;
        return landedThisFrame;
    }

    private bool TryHandleBufferedLandingJump(bool landedThisFrame)
    {
        if (!landedThisFrame || !input.HasBufferedJump || !CanGroundJump ||
            !IsLocomotionState(stateMachine.CurrentState))
            return false;

        if (stateMachine.CurrentState == stateMachine.Jump)
            return stateMachine.Jump.TryPerformBufferedJump();

        stateMachine.ChangeState(StateCharacter.Jump);
        return stateMachine.CurrentState == stateMachine.Jump && !input.HasBufferedJump;
    }

    private bool CanArmCoyoteTime()
    {
        IState state = stateMachine.CurrentState;
        return state == stateMachine.Idle || state == stateMachine.Move || state == stateMachine.Crouch;
    }

    private void ForfeitGroundJump()
    {
        groundJumpAvailable = false;
        coyoteTimeRemaining = 0f;
    }

    private void EnterAirborneLocomotionIfNeeded()
    {
        if (IsGrounded || !CanArmCoyoteTime())
            return;

        stateMachine.Jump.PrepareReentry();
        stateMachine.ChangeState(StateCharacter.Jump);
    }

    public void ConsumeEvadeInput() => input.ConsumeEvade();

    public bool TryPlayAnimation(int stateHash)
    {
        const int baseLayer = 0;

        if (Anim == null || Anim.layerCount <= baseLayer || !Anim.HasState(baseLayer, stateHash))
            return false;

        Anim.Play(stateHash, baseLayer, 0f);
        return true;
    }

    public bool TryPlayAnimation(string stateName, string fallbackStateName)
    {
        if (TryPlayAnimationStateName(stateName))
            return true;

        return TryPlayAnimationStateName(fallbackStateName);
    }

    public bool TrySetAnimatorFloat(int parameterHash, float value)
    {
        if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Float))
            return false;

        Anim.SetFloat(parameterHash, value);
        return true;
    }

    public bool TrySetAnimatorInt(int parameterHash, int value)
    {
        if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Int))
            return false;

        Anim.SetInteger(parameterHash, value);
        return true;
    }

    public bool TrySetAnimatorBool(int parameterHash, bool value)
    {
        if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Bool))
            return false;

        Anim.SetBool(parameterHash, value);
        return true;
    }

    public bool TrySetAnimatorTrigger(int parameterHash)
    {
        if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Trigger))
            return false;

        Anim.SetTrigger(parameterHash);
        return true;
    }

    private bool TryHandleDashInput()
    {
        if (!input.HasBufferedDash)
            return false;

        bool isCancellingHeavyCharge = stateMachine.CurrentState == stateMachine.HeavyCharge;
        bool canStartFromCurrentState = stateMachine.CurrentState == stateMachine.Idle ||
            stateMachine.CurrentState == stateMachine.Move ||
            stateMachine.CurrentState == stateMachine.Crouch ||
            isCancellingHeavyCharge;
        bool hasHorizontalDirection = Mathf.Abs(MoveInput.x) >= stats.tiltThreshold;

        if (!canStartFromCurrentState || !IsGrounded || !hasHorizontalDirection)
            return false;

        if (!Dash.TryStartDash(MoveInput.x))
            return false;

        input.ConsumeDash();

        if (!isCancellingHeavyCharge && input.HasBufferedAttack)
        {
            input.ConsumeAttack();
            stateMachine.ChangeState(StateCharacter.DashAttack);
            return true;
        }

        if (!isCancellingHeavyCharge && input.HasBufferedGrab)
        {
            input.ConsumeGrab();
            stateMachine.ChangeState(StateCharacter.DashGrab);
            return true;
        }

        stateMachine.ChangeState(StateCharacter.Dash);
        return true;
    }

    private bool TryHandleHeavyAttackInput()
    {
        if (!input.HasBufferedHeavyAttack)
            return false;

        bool canStartFromCurrentState = stateMachine.CurrentState == stateMachine.Idle ||
            stateMachine.CurrentState == stateMachine.Move ||
            stateMachine.CurrentState == stateMachine.Crouch;

        if (!IsGrounded || !canStartFromCurrentState)
            return false;

        HeavyAttackType attackType = Combat.ResolveHeavyAttackType();
        HeavyAttackStats heavyStats = Combat.GetHeavyAttackStats(attackType);
        if (heavyStats == null)
        {
            input.ConsumeHeavyAttack();
            Debug.LogError($"[PlayerController] Missing {attackType} Heavy Attack stats on '{gameObject.name}'.");
            return false;
        }

        input.ConsumeHeavyAttack();
        stateMachine.HeavyCharge.Prepare(attackType, heavyStats);
        stateMachine.ChangeState(StateCharacter.HeavyCharge);
        return true;
    }

    private void CacheAnimatorParameters()
    {
        animatorParameterTypes.Clear();
        if (Anim == null)
            return;

        foreach (AnimatorControllerParameter parameter in Anim.parameters)
            animatorParameterTypes[parameter.nameHash] = parameter.type;
    }

    private bool HasAnimatorParameter(int parameterHash, AnimatorControllerParameterType parameterType)
    {
        return Anim != null && animatorParameterTypes.TryGetValue(parameterHash, out AnimatorControllerParameterType actualType) &&
            actualType == parameterType;
    }

    private bool TryPlayAnimationStateName(string stateName)
    {
        if (Anim == null || string.IsNullOrWhiteSpace(stateName))
            return false;

        return TryPlayAnimation(Animator.StringToHash($"Base Layer.{stateName}"));
    }

    public void EnterDieState() => stateMachine.ChangeState(StateCharacter.Die);
}
