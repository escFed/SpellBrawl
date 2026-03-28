using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private FightingInputManager inputManager;
    private Rigidbody2D rb;
    private StateMachine stateMachine;
    private PlayerHitBox HitBox;

    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public JabState JabState { get; private set; }
    public ForwardTiltState ForwardTiltState { get; private set; }
    public UpTiltState UpTiltState { get; private set; }
    public DieState DieState { get; private set; }

    public Vector2 MoveInput => inputManager.CurrentDirection;
    public bool JumpPressed => inputManager.HasBufferedJump;
    public bool AttackInput => inputManager.HasBufferedAttack;
    public bool IsGrounded { get; private set; }
    public bool IsDead { get; private set; }

    public float stunTimer;

    private void Awake()
    {
        inputManager = GetComponent<FightingInputManager>();
        HitBox = GetComponent<PlayerHitBox>();
        rb = GetComponent<Rigidbody2D>();

        stateMachine = new StateMachine();
        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        JumpState = new JumpState(this, stateMachine);
        JabState = new JabState(this, stateMachine);
        ForwardTiltState = new ForwardTiltState(this, stateMachine);
        UpTiltState = new UpTiltState(this, stateMachine);
        DieState = new DieState(this, stateMachine);
    }

    private void Start() => stateMachine.Initialize(IdleState);

    private void Update()
    {
        if (IsDead) return;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        if (stunTimer > 0) return;

        stateMachine.FixedUpdate();
    }

    public void TakeHit(float stunDuration)
    {
        stunTimer = stunDuration;
        inputManager.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

    public IState ResolveAttackState()
    {
        Vector2 dir = inputManager.CurrentDirection;
        bool hasHorizontal = Mathf.Abs(dir.x) >= stats.tiltThreshold;
        bool hasUp = dir.y >= stats.tiltThreshold;

        inputManager.ConsumeAttack();

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return UpTiltState;
        if (hasHorizontal) return ForwardTiltState;
        return JabState;
    }

    public void ConsumeJump() => inputManager.ConsumeJump();

    public void ApplyHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * stats.moveSpeed, rb.linearVelocity.y);

        HitBox.CheckAndFlip(MoveInput.x);
    }

    public void StopHorizontalMovement() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

    public void ApplyJumpForce() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);

    public void OpenJabHitbox() => HitBox.SetJabHitbox(true);
    public void CloseJabHitbox() => HitBox.SetJabHitbox(false);
    public void OpenFTiltHitbox() => HitBox.SetFTiltHitbox(true);
    public void CloseFTiltHitbox() => HitBox.SetFTiltHitbox(false);
    public void OpenUTiltHitbox() => HitBox.SetUTiltHitbox(true);
    public void CloseUTiltHitbox() => HitBox.SetUTiltHitbox(false);

    public void OnDeath()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void Respawn(Vector3 position)
    {
        IsDead = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;
        inputManager.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }
}