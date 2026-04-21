using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;
    public bool IsGrounded { get; private set; }
    public bool IsDead { get; private set; }
    public int PlayerIndex { get; private set; }
    public float stunTimer;

    [Header("Cards")]
    public GameObject[] cardPrefabsPool;
    public int totalDeckSize = 20;
    private List<ICardable> reserveDeck = new List<ICardable>();
    private ICardable[] currentHand = new ICardable[5];

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private int playerId;

    public Transform throwPoint;
    private IInputProvider input;
    private Rigidbody2D rb;
    private PlayerHitBox HitBox;
    private EnergyManager energy;
    private TextMeshProUGUI deckCountText;

    private StateMachine stateMachine;
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public JabState JabState { get; private set; }
    public ForwardTiltState ForwardTiltState { get; private set; }
    public DownTiltState DownTiltState { get; private set; }
    public UpTiltState UpTiltState { get; private set; }
    public CardState CardState { get; private set; }
    public DieState DieState { get; private set; }

    public PlayerStats Stats => stats;
    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed =>  input.HasBufferedJump;
    public bool AttackInput => input.HasBufferedAttack;
    public int PlayerId => playerId;

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        HitBox = GetComponent<PlayerHitBox>();
        rb = GetComponent<Rigidbody2D>();
        energy = GetComponent<EnergyManager>();

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

        EnergyManager energy = GetComponent<EnergyManager>();
        if (energy != null && UIManager.Instance != null)
        {
            Slider energySlider = (PlayerIndex == 0) ? UIManager.Instance.p1_energySlider : UIManager.Instance.p2_energySlider;

            energy.SetUIElements(energySlider);
        }

        if (UIManager.Instance != null)
        {
            deckCountText = (PlayerIndex == 0) ? UIManager.Instance.p1_deckCountText : UIManager.Instance.p2_deckCountText;
        }

        ResetDeckForNewRound();
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

        if (input.HasBufferedDrawCards)
        {
            TryDrawNewHand();
            input.ConsumeDrawCards();
        }

        if (input.HasBufferedHand1) { TryUseCardFromHand(0); input.ConsumeHand1(); }
        else if (input.HasBufferedHand2) { TryUseCardFromHand(1); input.ConsumeHand2(); }
        else if (input.HasBufferedHand3) { TryUseCardFromHand(2); input.ConsumeHand3(); }
        else if (input.HasBufferedHand4) { TryUseCardFromHand(3); input.ConsumeHand4(); }
        else if (input.HasBufferedHand5) { TryUseCardFromHand(4); input.ConsumeHand5(); }

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (IsDead || stunTimer > 0) return;
        stateMachine.FixedUpdate();
    }

    public void ResetDeckForNewRound()
    {
        foreach (var card in currentHand) { if (card != null && card is MonoBehaviour mb) Destroy(mb.gameObject); }
        foreach (var card in reserveDeck) { if (card != null && card is MonoBehaviour mb) Destroy(mb.gameObject); }

        reserveDeck.Clear();
        for (int i = 0; i < currentHand.Length; i++) currentHand[i] = null;

        if (cardPrefabsPool != null && cardPrefabsPool.Length > 0)
        {
            for (int i = 0; i < totalDeckSize; i++)
            {
                GameObject randomPrefab = cardPrefabsPool[Random.Range(0, cardPrefabsPool.Length)];
                GameObject newCard = Instantiate(randomPrefab, transform.position, Quaternion.identity, transform);
                newCard.SetActive(false);
                reserveDeck.Add(newCard.GetComponent<ICardable>());
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (reserveDeck.Count > 0)
            {
                currentHand[i] = reserveDeck[0];
                reserveDeck.RemoveAt(0);
            }
        }

        UpdateHandUI();
        UpdateDeckCountUI();
    }

    public void TryUseCardFromHand(int handIndex)
    {
        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState) return;
        if (handIndex < currentHand.Length && currentHand[handIndex] != null)
        {
            ICardable cardToUse = currentHand[handIndex];

            if (energy != null && !energy.TrySpendEnergy(cardToUse.EnergyCost))
            {
                return;
            }
            if (cardToUse is MonoBehaviour mb)
            {
                mb.gameObject.SetActive(true);
            }

            CardState.SetCard(cardToUse, 0.5f);
            stateMachine.ChangeState(CardState);
            currentHand[handIndex] = null;
            UpdateHandUI();
        }
    }

    public void TryDrawNewHand()
    {
        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState) return;

        EnergyManager energy = GetComponent<EnergyManager>();
        if (energy == null || !energy.TrySpendEnergy(75))
        {
            return;
        }

        for (int i = 0; i < currentHand.Length; i++)
        {
            if (currentHand[i] != null)
            {
                if (currentHand[i] is MonoBehaviour mb) Destroy(mb.gameObject);
                currentHand[i] = null;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (reserveDeck.Count > 0)
            {
                currentHand[i] = reserveDeck[0];
                reserveDeck.RemoveAt(0);
            }
        }

        UpdateHandUI();
        UpdateDeckCountUI();
    }

    private void UpdateHandUI()
    {
        if (UIManager.Instance == null) return;
        Image[] UISlots = (PlayerIndex == 0) ? UIManager.Instance.p1_cards : UIManager.Instance.p2_cards;

        for (int i = 0; i < UISlots.Length; i++)
        {
            if (i < currentHand.Length && currentHand[i] != null)
            {
                UISlots[i].gameObject.SetActive(true);
                currentHand[i].SetUI(UISlots[i]);
            }
            else
            {
                UISlots[i].gameObject.SetActive(false);
            }
        }
    }
    private void UpdateDeckCountUI()
    {
        if (deckCountText != null)
        {
            deckCountText.text = reserveDeck.Count.ToString();
        }
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

    public void ApplyHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * stats.moveSpeed, rb.linearVelocity.y);
        HitBox.CheckAndFlip(MoveInput.x);
    }

    public void StopHorizontalMovement() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    public void ApplyJumpForce() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
    public void ConsumeJump() => input.ConsumeJump();
    public void EnterDieState() => stateMachine.ChangeState(DieState);

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

        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent(out Collider2D col)) col.enabled = false;

        if (GameManager.Instance != null) GameManager.Instance.PlayerDied(PlayerIndex);
    }

    public void Respawn(Vector3 position)
    {
        IsDead = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;

        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = true;
        if (TryGetComponent(out Collider2D col)) col.enabled = true;

        input.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        int reducedDamage = DamageManager.CalculateDamage(amount);
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