using UnityEngine;

public class CharacterAI : MonoBehaviour, IInputProvider
{
    [Header("Settings")]
    public float attackRange = 1.2f;
    public float cardRange = 6f;
    public float reactionTime = 0.15f;

    [Header("Fall check")]
    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 5f;
    public float gapJumpDistance = 4f;

    public PlayerController SelfController { get; private set; }
    public EnergyManager SelfEnergy { get; private set; }
    public CharacterHealth SelfHealth { get; private set; }
    public CharacterDeck SelfDeck { get; private set; }
    public Transform Target { get; private set; }

    public AIDecision currentDecision = AIDecision.Search;
    private AIDecision previousDecision;

    private float thinkTimer;
    private int selectedCardIndex = -1;
    private float cardTimer;

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
    public bool IsShieldHeld => false; 

    private void Awake()
    {
        SelfController = GetComponent<PlayerController>();
        SelfEnergy = GetComponent<EnergyManager>();
        SelfHealth = GetComponent<CharacterHealth>();
        SelfDeck = GetComponent<CharacterDeck>();
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

        if (cardTimer > 0) cardTimer -= Time.deltaTime;

        thinkTimer -= Time.deltaTime;
        if (thinkTimer <= 0)
        {
            thinkTimer = reactionTime;
            EvaluateSituation();
            ExecuteDecision();

            if (currentDecision != previousDecision)
            {
                
                previousDecision = currentDecision;
            }
        }
    }
    private void EvaluateSituation()
    {
        float distance = Vector2.Distance(transform.position, Target.position);
        float damage = SelfHealth != null ? SelfHealth.currentDamage : 0f;

        if (HasEmptyHand() && SelfEnergy != null && SelfEnergy.currentEnergy >= 75)
        {
            currentDecision = AIDecision.DrawCards;
            return;
        }

        if (distance > 15f)
        {
            currentDecision = AIDecision.Search;
            return;
        }

        if (damage > 80f)
        {
            if (TryFindCardType(CardType.Defensive)) { currentDecision = AIDecision.UseDefensiveCard; return; }
            if (distance < attackRange * 2) { currentDecision = AIDecision.Flee; return; }
        }

        if (SelfEnergy != null && SelfEnergy.currentEnergy >= 30 && cardTimer <= 0)
        {
            float probability = SelfEnergy.currentEnergy / 100f;

            if (Random.value <= probability)
            {
                if (distance > attackRange && distance <= cardRange && TryFindCardType(CardType.Utility))
                {
                    currentDecision = AIDecision.UseUtilityCard; return;
                }

                if (distance > attackRange && TryFindCardType(CardType.Offensive))
                {
                    currentDecision = AIDecision.UseOffensiveCard; return;
                }
            }
            else
            {
                cardTimer = 1.0f;
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
                bool jumpToChase = false;
                if (IsSafeToMove(dirX, out jumpToChase))
                {
                    CurrentDirection = new Vector2(dirX, 0);
                    if (jumpToChase) HasBufferedJump = true;
                }
                else
                {
                    CurrentDirection = Vector2.zero;
                }
                break;

            case AIDecision.Flee:
                bool jumpToFlee = false;
                if (IsSafeToMove(-dirX, out jumpToFlee))
                {
                    CurrentDirection = new Vector2(-dirX, 0);
                    if (jumpToFlee) HasBufferedJump = true;
                }
                else
                {
                    CurrentDirection = Vector2.zero;
                }
                break;

            case AIDecision.Attack:
                CurrentDirection = new Vector2(dirX, 0);
                HasBufferedAttack = true;
                break;

            case AIDecision.Parry:
                CurrentDirection = Vector2.zero;
                HasBufferedParry = true;
                break;

            case AIDecision.DrawCards:
                CurrentDirection = Vector2.zero;
                HasBufferedDrawCards = true;
                break;

            case AIDecision.UseOffensiveCard:
            case AIDecision.UseDefensiveCard:
            case AIDecision.UseUtilityCard:
                CurrentDirection = Vector2.zero;
                PressCardButton(selectedCardIndex);
                break;
        }
    }

    private bool TryFindCardType(CardType targetType)
    {
        ICardable[] hand = SelfDeck.GetCurrentHand();

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

    private bool HasEmptyHand()
    {
        ICardable[] hand = SelfDeck.GetCurrentHand();
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] != null) return false;
        }
        return true;
    }

    private void PressCardButton(int index)
    {
        if (index == 0) HasBufferedHand1 = true;
        else if (index == 1) HasBufferedHand2 = true;
        else if (index == 2) HasBufferedHand3 = true;
        else if (index == 3) HasBufferedHand4 = true;
        else if (index == 4) HasBufferedHand5 = true;
    }

    public bool IsSafeToMove(float directionX, out bool shouldJump)
    {
        shouldJump = false;

        if (!SelfController.IsGrounded) return true;

        Vector2 edgeCheck = new Vector2(transform.position.x + (directionX * lookAheadDistance), transform.position.y);
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheck, Vector2.down, fallCheckDepth, groundLayer);

        if (edgeHit.collider != null) return true;

        Vector2 gapCheck = new Vector2(transform.position.x + (directionX * gapJumpDistance), transform.position.y);
        RaycastHit2D gapHit = Physics2D.Raycast(gapCheck, Vector2.down, fallCheckDepth, groundLayer);

        if (gapHit.collider != null)
        {
            shouldJump = true;
            return true;
        }

        return false;
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