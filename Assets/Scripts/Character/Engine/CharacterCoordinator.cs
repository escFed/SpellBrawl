using UnityEngine;

public class CharacterCoordinator : MonoBehaviour
{
    [Header("Stats")]
    public CharacterStats stats;

    [Header("Control")]
    [SerializeField] private PlayerMode defaultPlayerMode = PlayerMode.Player;

    public CharacterStats Stats => stats;
    public PlayerController Controller { get; private set; }
    public PlayerMode Mode => Controller != null ? Controller.Mode : defaultPlayerMode;
    public PlayerSlot Slot { get; private set; }
    public bool IsDead => Health != null && Health.IsDead;
    public int PlayerIndex
    {
        get => (int)Slot;
        set => Slot = (PlayerSlot)Mathf.Clamp(value, (int)PlayerSlot.PlayerOne, (int)PlayerSlot.PlayerTwo);
    }
    public bool IsParrying => Parry != null && Parry.IsParrying;
    public bool IsIntangible => Health != null && Health.IsIntangible;
    public bool IsHitStunned => stateMachine != null && States != null && stateMachine.Is(States.HitStun);

    public int JumpsRemaining => jumpController.JumpsRemaining;
    public bool CanGroundJump => jumpController.CanGroundJump;
    public bool CanJump => jumpController.CanJump;
    public float CoyoteTimeRemaining => jumpController.CoyoteTimeRemaining;

    private bool wasPaused;
    [SerializeField] private bool controlsEnabled = true;
    public bool cardsEnabled = true;

    private CharacterDeck deck;
    private CharacterActionRouter actionRouter;
    private CharacterJumpController jumpController;
    private IInputProvider input => ActiveInput;

    public Animator Anim { get; private set; }
    public CharacterAnimationController Animation { get; private set; }
    public CharacterCombat Combat { get; private set; }
    public CharacterParry Parry { get; private set; }
    public CharacterGrab Grab { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterHealth Health { get; private set; }
    public CharacterShield Shield { get; private set; }
    public CharacterRoll Roll { get; private set; }
    public CharacterDodge Dodge { get; private set; }
    public CharacterDash Dash { get; private set; }
    public CharacterHitFeedback HitFeedback { get; private set; }
    public CharacterStateMachine stateMachine { get; private set; }
    public CharacterStates States { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public CharacterVisuals Visuals { get; private set; }

    public IInputProvider ActiveInput => Controller?.ActiveInput;
    public Vector2 MoveInput => ActiveInput != null ? ActiveInput.CurrentDirection : Vector2.zero;
    public bool JumpPressed => ActiveInput != null && ActiveInput.HasBufferedJump;
    public bool IsGrounded => Movement.IsGrounded;
    public bool AttackInput => ActiveInput != null && ActiveInput.HasBufferedAttack;
    public bool GrabInput => ActiveInput != null && ActiveInput.HasBufferedGrab;
    public bool EvadePressed => ActiveInput != null && ActiveInput.HasBufferedEvade;
    public bool DashPressed => ActiveInput != null && ActiveInput.HasBufferedDash;
    public bool HeavyAttackPressed => ActiveInput != null && ActiveInput.HasBufferedHeavyAttack;

    private void Awake()
    {
        deck = GetComponent<CharacterDeck>();
        Parry = GetComponent<CharacterParry>();
        Combat = GetComponent<CharacterCombat>();
        Grab = GetComponent<CharacterGrab>();
        Movement = GetComponent<CharacterMovement>();
        Health = GetComponent<CharacterHealth>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
        Visuals = GetComponent<CharacterVisuals>();
        Anim = GetComponentInChildren<Animator>();
        Animation = new CharacterAnimationController(Anim, this);
        Shield = GetComponent<CharacterShield>();
        Roll = GetComponent<CharacterRoll>();
        Dodge = GetComponent<CharacterDodge>();
        Dash = GetComponent<CharacterDash>();
        HitFeedback = GetComponent<CharacterHitFeedback>();
        Controller = new PlayerController(GetComponent<CharacterBrain>(), GetComponent<CharacterAI>(), controlsEnabled);

        if (Visuals == null)
            Visuals = gameObject.AddComponent<CharacterVisuals>();

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

        if (!Controller.Configure(defaultPlayerMode))
            Debug.LogError($"[CharacterCoordinator] Missing input provider for {defaultPlayerMode} on '{name}'.", this);

        stateMachine = new CharacterStateMachine();
        States = new CharacterStates(this, stateMachine);
        actionRouter = new CharacterActionRouter(this, deck, Parry);
        jumpController = new CharacterJumpController(this);

        if (GetComponent<IGrabbable>() == null)
            gameObject.AddComponent<CharacterGrabbable>();
    }

    private void Start()
    {
        Movement.RefreshGroundedState();
        stateMachine.ChangeState(States.Idle);
        jumpController.Initialize();
    }

    public ICharacterState GetCurrentState() => stateMachine.CurrentState;
    public void ChangeState(ICharacterState newState) => stateMachine.ChangeState(newState);

    public bool ConfigureControl(PlayerSlot slot, PlayerMode mode)
    {
        Slot = slot;
        defaultPlayerMode = mode;

        if (Controller == null || !Controller.Configure(mode))
        {
            Debug.LogError($"[CharacterCoordinator] Cannot configure {slot} as {mode} on '{name}'.", this);
            return false;
        }

        return true;
    }

    public bool ConfigureInputProvider(IInputProvider inputProvider)
    {
        return Controller != null && Controller.ConfigureInputProvider(inputProvider);
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        Controller?.SetInputEnabled(enabled);
    }

    private void Update()
    {
        IInputProvider input = ActiveInput;
        if (input == null)
            return;

        if (IsDead || stateMachine.Is(States.Die))
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

        if (!Controller.InputEnabled)
        {
            input.ClearAllInputs();
            return;
        }

        if (CombatFeedback.IsHitStopActive) return;

        Movement.RefreshGroundedState();
        if (jumpController.Tick())
            return;

        ICharacterState stateBeforeUpdate = stateMachine.CurrentState;

        if (IsHitStunned)
        {
            stateMachine.Update();
            TryExecuteBufferedInputAfterRecovery(stateBeforeUpdate);
            return;
        }

        if (actionRouter.TryHandlePriorityInputs())
            return;

        stateMachine.Update();
        TryExecuteBufferedInputAfterRecovery(stateBeforeUpdate);
    }

    private void TryExecuteBufferedInputAfterRecovery(ICharacterState previousState)
    {
        // Attack recovery and hitstun can end inside CharacterStateMachine.Update. Dispatch once
        // more so a still-buffered command runs on that same newly legal frame.
        if (!WasBufferedRecoveryState(previousState) || previousState == stateMachine.CurrentState ||
            !IsLocomotionState(stateMachine.CurrentState))
            return;

        if (actionRouter.TryHandlePriorityInputs())
            return;

        if (HasBufferedLocomotionInput(stateMachine.CurrentState))
            stateMachine.Update();
    }

    private bool WasBufferedRecoveryState(ICharacterState state)
    {
        return state == States.HitStun || state is AttackState;
    }

    private bool IsLocomotionState(ICharacterState state)
    {
        return state == States.Idle || state == States.Move ||
            state == States.Crouch || state == States.Jump;
    }

    private bool HasBufferedLocomotionInput(ICharacterState state)
    {
        if (state == States.Idle || state == States.Move)
            return input.HasBufferedGrab || input.HasBufferedAttack ||
                (input.HasBufferedJump && CanJump);

        if (state == States.Crouch)
            return input.HasBufferedJump && CanJump;

        if (state == States.Jump)
            return input.HasBufferedAttack ||
                (input.HasBufferedJump && CanJump);

        return false;
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        if (PauseMenu.isPaused) return;
        Movement.StepPhysics();
        if (!Controller.InputEnabled) return;
        stateMachine.FixedUpdate();
    }
    public void ExecuteCardState(ICardable cardToUse)
    {
        States.Card.SetCard(cardToUse, 0.5f);
        stateMachine.ChangeState(States.Card);
    }

    public bool TryPerformJump()
    {
        return jumpController.TryPerformJump();
    }

    public void HandleAirborneMovementInput()
    {
        jumpController.HandleAirborneMovementInput();
    }

    public void ResetJumps()
    {
        jumpController.ResetJumps();
    }

    public void CancelGroundJumpAvailability()
    {
        jumpController.CancelGroundJumpAvailability();
    }

    public void ConsumeEvadeInput() => input.ConsumeEvade();

    public void EnterDieState() => stateMachine.ChangeState(States.Die);
}
