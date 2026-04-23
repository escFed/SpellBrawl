using UnityEngine;

public class PlayerAI : MonoBehaviour, IInputProvider
{
    [Header("Settings")]
    public float attackRange = 1.2f;
    public float cardRange = 6f;
    public float reactionTime = 0.15f;

    [Header("Fall check")]
    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 2f;

    public PlayerController SelfController { get; private set; }
    public EnergyManager SelfEnergy { get; private set; }
    public PlayerHealth SelfHealth { get; private set; }
    public Transform Target { get; private set; }

    public AIDecision currentDecision = AIDecision.Search;
    private float thinkTimer;
    private int selectedCardIndex = -1;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }
    public bool HasBufferedHand3 { get; private set; }
    public bool HasBufferedHand4 { get; private set; }
    public bool HasBufferedHand5 { get; private set; }
    public bool HasBufferedParry { get; private set; }
    public bool HasBufferedDrawCards { get; private set; }

    private void Awake()
    {
        SelfController = GetComponent<PlayerController>();
        SelfEnergy = GetComponent<EnergyManager>();
        SelfHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        Invoke(nameof(FindTarget), 0.5f);
    }

    private void FindTarget()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
        {
            if (p != SelfController) { Target = p.transform; break; }
        }
    }

    private void Update()
    {
        if (SelfController.IsDead || SelfController.stunTimer > 0)
        {
            ClearAllInputs();
            return;
        }

        if (Target == null)
        {
            FindTarget();
            if (Target == null) return;
        }

        thinkTimer -= Time.deltaTime;
        if (thinkTimer <= 0)
        {
            thinkTimer = reactionTime;
            EvaluateSituation();
            ExecuteDecision();
        }
    }
    private void EvaluateSituation()
    {
        float distance = Vector2.Distance(transform.position, Target.position);
        float myDamage = SelfHealth != null ? SelfHealth.currentDamage : 0f;

        if (distance > 15f)
        {
            currentDecision = AIDecision.Search;
            return;
        }

        if (myDamage > 80f)
        {
            if (TryFindCardCategory(CardType.Defensive)) { currentDecision = AIDecision.UseDefensiveCard; return; }
            if (distance < attackRange * 2) { currentDecision = AIDecision.Flee; return; }
        }

        if (SelfEnergy != null && SelfEnergy.currentEnergy >= 30)
        {
            if (distance > attackRange && distance <= cardRange && TryFindCardCategory(CardType.Utility))
            {
                currentDecision = AIDecision.UseUtilityCard; return;
            }

            if (distance > attackRange && TryFindCardCategory(CardType.Offensive))
            {
                currentDecision = AIDecision.UseOffensiveCard; return;
            }
        }

        if (distance <= attackRange)
        {
            if (Random.value < 0.25f && !SelfController.IsParrying)
            {
                currentDecision = AIDecision.Parry; return;
            }

            currentDecision = AIDecision.Attack; return;
        }

        currentDecision = AIDecision.Chase;
    }

    private void ExecuteDecision()
    {
        ClearAllInputs();
        float dirX = Mathf.Sign(Target.position.x - transform.position.x);

        switch (currentDecision)
        {
            case AIDecision.Search:
                CurrentDirection = Vector2.zero;
                break;

            case AIDecision.Chase:
                if (IsSafeToMove(dirX)) CurrentDirection = new Vector2(dirX, 0);
                else { CurrentDirection = Vector2.zero; HasBufferedJump = true; }
                break;

            case AIDecision.Flee:
                if (IsSafeToMove(-dirX)) CurrentDirection = new Vector2(-dirX, 0);
                else { CurrentDirection = Vector2.zero; HasBufferedJump = true; }
                break;

            case AIDecision.Attack:
                CurrentDirection = new Vector2(dirX, 0);
                HasBufferedAttack = true;
                break;

            case AIDecision.Parry:
                CurrentDirection = Vector2.zero;
                HasBufferedParry = true;
                break;

            case AIDecision.UseOffensiveCard:
            case AIDecision.UseDefensiveCard:
            case AIDecision.UseUtilityCard:
                CurrentDirection = Vector2.zero;
                PressCardButton(selectedCardIndex);
                break;
        }
    }

    private bool TryFindCardCategory(CardType targetType)
    {
        ICardable[] hand = SelfController.GetCurrentHand();

        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == null) continue;
            if (SelfEnergy.currentEnergy < hand[i].EnergyCost) continue;
            if (!hand[i].CanBeUsed(SelfController)) continue;

            if (hand[i].Type == targetType)
            {
                selectedCardIndex = i;
                return true;
            }
        }
        return false;
    }

    private void PressCardButton(int index)
    {
        if (index == 0) HasBufferedHand1 = true;
        else if (index == 1) HasBufferedHand2 = true;
        else if (index == 2) HasBufferedHand3 = true;
        else if (index == 3) HasBufferedHand4 = true;
        else if (index == 4) HasBufferedHand5 = true;
    }

    public bool IsSafeToMove(float directionX)
    {
        if (!SelfController.IsGrounded) return true;
        Vector2 rayOrigin = new Vector2(transform.position.x + (directionX * lookAheadDistance), transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, fallCheckDepth, groundLayer);
        return hit.collider != null;
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeParry() => HasBufferedParry = false;
    public void ConsumeDrawCards() => HasBufferedDrawCards = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;
    public void ConsumeHand3() => HasBufferedHand3 = false;
    public void ConsumeHand4() => HasBufferedHand4 = false;
    public void ConsumeHand5() => HasBufferedHand5 = false;

    public void ClearAllInputs()
    {
        ConsumeJump(); 
        ConsumeAttack(); 
        ConsumeParry(); 
        ConsumeDrawCards();
        ConsumeHand1(); ConsumeHand2(); ConsumeHand3(); ConsumeHand4(); ConsumeHand5();
        CurrentDirection = Vector2.zero;
    }
}