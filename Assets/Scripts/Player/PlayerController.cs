using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{


    [Header("Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Cards")]
    public GameObject[] DeckSlots = new GameObject[4];

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Player Settings")]
    [SerializeField] private int playerId;

    private IInputProvider input;
    private Rigidbody2D rb;
    private StateMachine stateMachine;
    private PlayerHitBox HitBox;
    private Queue<ICardable> reserveDeck = new Queue<ICardable>();
    private ICardable[] currentHand = new ICardable[2];
   
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public JabState JabState { get; private set; }
    public ForwardTiltState ForwardTiltState { get; private set; }
    public DownTiltState DownTiltState { get; private set; }
    public UpTiltState UpTiltState { get; private set; }
    public CardState CardState { get; private set; }
    public DieState DieState { get; private set; }

    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed => input.HasBufferedJump;
    public bool AttackInput => input.HasBufferedAttack;
    public bool IsGrounded { get; private set; }
    public bool IsDead { get; private set; }
    public int PlayerIndex { get; private set; }

    public int PlayerId => playerId;

    public float stunTimer;
    public Transform throwPoint;


    public Transform starThrowPoint;





   

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        HitBox = GetComponent<PlayerHitBox>();
        rb = GetComponent<Rigidbody2D>();

        stateMachine = new StateMachine();
        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        JumpState = new JumpState(this, stateMachine);
        JabState = new JabState(this, stateMachine);
        ForwardTiltState = new ForwardTiltState(this, stateMachine);
        UpTiltState = new UpTiltState(this, stateMachine);
        DownTiltState = new DownTiltState(this, stateMachine);
        DieState = new DieState(this, stateMachine);
        CardState = new CardState(this, stateMachine);
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        PlayerInput playerInput = GetComponent<PlayerInput>();
        PlayerIndex = playerInput != null ? playerInput.playerIndex : 1;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null && UIManager.Instance != null)
        {

            health.Init(playerId);
            TextMeshProUGUI myText = (PlayerIndex == 0) ? UIManager.Instance.p1_damageText : UIManager.Instance.p2_damageText;
            GameObject[] myLines = (PlayerIndex == 0) ? UIManager.Instance.p1_life : UIManager.Instance.p2_life;

            health.SetUIElements(myText, myLines);
        }

        foreach (GameObject cardObj in DeckSlots)
        {
            if (cardObj != null)
            {
                ICardable cardComponent = cardObj.GetComponent<ICardable>();
                if (cardComponent != null) reserveDeck.Enqueue(cardComponent);
            }
        }

        if (reserveDeck.Count >= 2)
        {
            currentHand[0] = reserveDeck.Dequeue();
            currentHand[1] = reserveDeck.Dequeue();
        }

        UpdateHandUI();
    }

   

    private void UpdateHandUI()
    {
        if (UIManager.Instance == null) return;

        Image[] UISlots = (PlayerIndex == 0) ? UIManager.Instance.p1_cards : UIManager.Instance.p2_cards;

        foreach (Image img in UISlots) img.gameObject.SetActive(false);

        if (currentHand[0] != null)
        {
            UISlots[0].gameObject.SetActive(true);
            currentHand[0].SetUI(UISlots[0]);
        }
        if (currentHand[1] != null)
        {
            UISlots[1].gameObject.SetActive(true);
            currentHand[1].SetUI(UISlots[1]);
        }
    }

    private void Update()
    {
        if (IsDead) return;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }
        if (input.HasBufferedHand1)
        {
            TryUseCardFromHand(0);
            input.ConsumeHand1();
        }
        else if (input.HasBufferedHand2)
        {
            TryUseCardFromHand(1);
            input.ConsumeHand2();
        }

        stateMachine.Update();
    }

    public void TryUseCardFromHand(int handIndex)
    {
        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState) return;

        if (currentHand[handIndex] != null)
        {
            ICardable cardToUse = currentHand[handIndex];

            CardState.SetCard(cardToUse, 0.5f);
            stateMachine.ChangeState(CardState);

            if (reserveDeck.Count > 0)
            {
                reserveDeck.Enqueue(cardToUse);
                currentHand[handIndex] = reserveDeck.Dequeue();
            }

            UpdateHandUI();
        }
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
        input.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

    public IState ResolveAttackState()
    {
        Vector2 dir = input.CurrentDirection;
        bool hasHorizontal = Mathf.Abs(dir.x) >= stats.tiltThreshold;
        bool hasUp = dir.y >= stats.tiltThreshold;

        bool hasDown = dir.y <= -stats.tiltThreshold;

        input.ConsumeAttack();

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return UpTiltState;

        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return DownTiltState;

        if (hasHorizontal) return ForwardTiltState;

        return JabState;
    }

    public void ConsumeJump() => input.ConsumeJump();

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
    public void OpenDTiltHitbox() => HitBox.SetDTiltHitbox(true);
    public void CloseDTiltHitbox() => HitBox.SetDTiltHitbox(false);

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
        input.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        Vector2 reducedKnockback = DamageManager.CalculateKnockback(playerId, knockback);
        GetComponent<PlayerHealth>().TakeDamage(amount, knockback);
    }

    public void ActivateHeal(Vector2 reduction, float duration)
    {
        DamageManager.AddKnockbackReduction(PlayerId, reduction, duration);
    }

    public int GetPlayerId()
    {
        return playerId;
    }
}


