using UnityEngine;

public class CharacterAI : MonoBehaviour, IInputProvider
{
    [Header("AI Profile")]
    public AIProfile profile = new AIProfile();

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

    public PlayerController SelfController { get; private set; }
    public EnergyManager SelfEnergy { get; private set; }
    public CharacterHealth SelfHealth { get; private set; }
    public CharacterDeck SelfDeck { get; private set; }
    public Transform Target { get; private set; }

    public AIDecision currentDecision = AIDecision.Idle;

    private PlayerController targetController;
    private Vector3 perceivedTargetPosition;
    private float thinkTimer;
    private float cardTimer;
    private int selectedCardIndex = -1;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedGrab { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }
    public bool HasBufferedHand3 { get; private set; }
    public bool HasBufferedHand4 { get; private set; }
    public bool HasBufferedParry { get; private set; }
    public bool HasBufferedShield { get; private set; }
    public bool HasBufferedEvade { get; private set; }
    public bool HasBufferedDash { get; private set; }
    public bool IsShieldHeld { get; private set; }
    public bool HasBufferedDrawCards { get; private set; }
    public bool HasBufferedHeavyAttack { get; private set; }
    public bool IsHeavyAttackHeld { get; private set; }
    public bool WasHeavyAttackReleased { get; private set; }

    private void Awake()
    {
        SelfController = GetComponent<PlayerController>();
        SelfEnergy = GetComponent<EnergyManager>();
        SelfHealth = GetComponent<CharacterHealth>();
        SelfDeck = GetComponent<CharacterDeck>();
        perceivedTargetPosition = transform.position;
    }

    private void Update()
    {
        if (SelfController.IsDead || SelfController.stunTimer > 0)
        {
            ClearAllInputs();
            return;
        }

        UpdateTargetTracker();

        if (Target == null)
        {
            ClearAllInputs();
            return;
        }

        if (cardTimer > 0f)
            cardTimer -= Time.deltaTime;

        thinkTimer -= Time.deltaTime;

        if (thinkTimer <= 0f)
        {
            thinkTimer = profile.reactionTime;
            perceivedTargetPosition = Target.position;

            EvaluateSituation();
            ExecuteDecision();
        }
    }

    private void UpdateTargetTracker()
    {
        if (targetController != null && !targetController.IsDead && targetController.gameObject.activeInHierarchy)
            return;

        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        targetController = null;
        Target = null;

        foreach (PlayerController player in allPlayers)
        {
            if (player != SelfController && !player.IsDead)
            {
                targetController = player;
                Target = player.transform;
                perceivedTargetPosition = Target.position;
                return;
            }
        }
    }

    private AIContext BuildContext()
    {
        float distanceX = Mathf.Abs(perceivedTargetPosition.x - transform.position.x);
        float distanceY = perceivedTargetPosition.y - transform.position.y;

        CharacterHealth targetHealth = Target != null ? Target.GetComponent<CharacterHealth>() : null;

        bool nearEdge = IsNearEdge();
        bool tooLow = transform.position.y < perceivedTargetPosition.y + recoveryHeightThreshold;

        return new AIContext
        {
            distanceX = distanceX,
            distanceY = distanceY,
            selfDamage = SelfHealth != null ? SelfHealth.currentDamage : 0f,
            targetDamage = targetHealth != null ? targetHealth.currentDamage : 0f,
            energy = SelfEnergy != null ? SelfEnergy.currentEnergy : 0f,
            targetInAttackRange = distanceX <= attackRange && Mathf.Abs(distanceY) < 1f,
            targetInCardRange = distanceX <= cardRange,
            targetAbove = distanceY > verticalJumpThreshold,
            targetBelow = distanceY < -verticalJumpThreshold,
            emptyHand = HasEmptyHand(),
            inDanger = SelfHealth != null && SelfHealth.currentDamage >= 85f,
            nearEdge = nearEdge,
            shouldRecover = tooLow || (nearEdge && SelfHealth != null && SelfHealth.currentDamage >= 100f)
        };
    }

    private void EvaluateSituation()
    {
        selectedCardIndex = -1;

        AIContext ctx = BuildContext();

        float bestScore = -999f;
        AIDecision bestDecision = AIDecision.Idle;

        Consider(AIDecision.Recover, ScoreRecover(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.UseDefensiveCard, ScoreDefensiveCard(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.UseOffensiveCard, ScoreOffensiveCard(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.UseUtilityCard, ScoreUtilityCard(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.DrawCards, ScoreDrawCards(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Parry, ScoreParry(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Attack, ScoreAttack(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Jump, ScoreJump(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Flee, ScoreFlee(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Reposition, ScoreReposition(ctx), ref bestScore, ref bestDecision);
        Consider(AIDecision.Chase, ScoreChase(ctx), ref bestScore, ref bestDecision);

        if (Random.value < profile.mistakeChance)
            bestDecision = AIDecision.Chase;

        currentDecision = bestDecision;
    }

    private void Consider(AIDecision decision, float score, ref float bestScore, ref AIDecision bestDecision)
    {
        score += Random.Range(-profile.randomness, profile.randomness);

        if (score > bestScore)
        {
            bestScore = score;
            bestDecision = decision;
        }
    }

    private float ScoreRecover(AIContext context)
    {
        if (!context.shouldRecover)
            return 0f;

        return 100f * profile.recoveryFocus;
    }

    private float ScoreAttack(AIContext context)
    {
        if (!context.targetInAttackRange)
            return 0f;

        float score = 55f;

        if (context.targetDamage >= 100f)
            score += 20f;

        if (context.selfDamage >= 120f)
            score -= 15f;

        return score * profile.aggression;
    }

    private float ScoreChase(AIContext context)
    {
        if (context.distanceX <= idealSpacing)
            return 0f;

        if (context.inDanger && context.distanceX < attackRange * 2f)
            return 8f;

        return 28f * profile.aggression;
    }

    private float ScoreFlee(AIContext context)
    {
        float score = 0f;

        if (context.inDanger)
            score += 35f;

        if (context.targetInAttackRange)
            score += 30f;

        if (context.nearEdge)
            score += 20f;

        return score * profile.defense;
    }

    private float ScoreJump(AIContext context)
    {
        if (context.targetAbove && context.distanceX < 4f)
            return 50f;

        return 0f;
    }

    private float ScoreReposition(AIContext context)
    {
        if (context.distanceX < attackRange && context.selfDamage > 70f)
            return 35f;

        if (context.distanceX < idealSpacing)
            return 20f;

        return 0f;
    }

    private float ScoreParry(AIContext context)
    {
        if (!context.targetInAttackRange)
            return 0f;

        if (SelfController.IsParrying)
            return 0f;

        float score = 25f;

        if (context.inDanger)
            score += 20f;

        if (context.energy < 40f)
            score += 15f;

        return score * profile.parrySkill;
    }

    private float ScoreDrawCards(AIContext context)
    {
        if (SelfEnergy == null || SelfEnergy.currentEnergy < 75f)
            return 0f;

        if (context.emptyHand)
            return 80f;

        if (!HasUsefulCard(context) && context.energy >= 100f)
            return 45f;

        return 0f;
    }

    private float ScoreOffensiveCard(AIContext context)
    {
        if (SelfEnergy == null || cardTimer > 0f)
            return 0f;

        if (!context.targetInCardRange)
            return 0f;

        if (!TryFindCardType(CardType.Offensive))
            return 0f;

        float score = 45f;

        if (context.distanceX > attackRange)
            score += 15f;

        if (context.targetDamage >= 80f)
            score += 20f;

        if (context.energy >= 80f)
            score += 10f;

        return score * profile.cardUsage;
    }

    private float ScoreDefensiveCard(AIContext context)
    {
        if (SelfEnergy == null || cardTimer > 0f)
            return 0f;

        if (!TryFindCardType(CardType.Defensive))
            return 0f;

        float score = 0f;

        if (context.inDanger)
            score += 60f;

        if (context.targetInAttackRange)
            score += 25f;

        if (context.nearEdge)
            score += 20f;

        return score * profile.defense;
    }

    private float ScoreUtilityCard(AIContext context)
    {
        if (SelfEnergy == null || cardTimer > 0f)
            return 0f;

        if (!TryFindCardType(CardType.Utility))
            return 0f;

        float score = 20f;

        if (context.targetAbove)
            score += 25f;

        if (context.shouldRecover)
            score += 40f;

        if (context.distanceX > attackRange && context.distanceX <= cardRange)
            score += 15f;

        return score * profile.cardUsage;
    }

    private void ExecuteDecision()
    {
        ClearAllInputs();

        float dirX = Mathf.Sign(perceivedTargetPosition.x - transform.position.x);
        if (dirX == 0f)
            dirX = transform.localScale.x >= 0f ? 1f : -1f;

        switch (currentDecision)
        {
            case AIDecision.Idle:
                CurrentDirection = Vector2.zero;
                break;

            case AIDecision.Chase:
                ExecuteMove(dirX, false);
                break;

            case AIDecision.Flee:
                ExecuteMove(-dirX, false);
                break;

            case AIDecision.Reposition:
                ExecuteMove(-dirX, false);
                break;

            case AIDecision.Recover:
                ExecuteMove(dirX, true);
                break;

            case AIDecision.Jump:
                CurrentDirection = new Vector2(dirX * 0.5f, 0f);
                HasBufferedJump = true;
                break;

            case AIDecision.Attack:
                CurrentDirection = new Vector2(dirX, 0f);
                HasBufferedAttack = true;
                break;

            case AIDecision.Parry:
                HasBufferedParry = true;
                break;

            case AIDecision.DrawCards:
                HasBufferedDrawCards = true;
                break;

            case AIDecision.UseOffensiveCard:
            case AIDecision.UseDefensiveCard:
            case AIDecision.UseUtilityCard:
                CurrentDirection = new Vector2(dirX, 0f);
                PressCardButton(selectedCardIndex);
                cardTimer = 0.7f;
                break;
        }
    }

    private void ExecuteMove(float directionX, bool forceJump)
    {
        if (Mathf.Abs(directionX) < 0.01f)
        {
            CurrentDirection = Vector2.zero;
            return;
        }

        bool shouldJump;
        bool safe = IsSafeToMove(directionX, out shouldJump);

        if (!safe)
        {
            CurrentDirection = Vector2.zero;
            return;
        }

        CurrentDirection = new Vector2(directionX, 0f);

        if (forceJump || shouldJump)
            HasBufferedJump = true;
    }

    private bool IsSafeToMove(float directionX, out bool shouldJump)
    {
        shouldJump = false;

        if (!SelfController.IsGrounded)
            return true;

        Vector2 edgeCheck = new Vector2(transform.position.x + directionX * lookAheadDistance, transform.position.y);
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheck, Vector2.down, fallCheckDepth, groundLayer);

        if (edgeHit.collider == null)
        {
            // Hay un borde: solo avanzar si el objetivo está al otro lado Y hay distancia suficiente para saltar
            float targetDist = perceivedTargetPosition.x - transform.position.x;
            bool targetBeyondEdge = Mathf.Sign(targetDist) == directionX && Mathf.Abs(targetDist) > lookAheadDistance * 2f;

            if (targetBeyondEdge)
            {
                shouldJump = true;
                return true;
            }

            return false; // Borde sin destino claro: no avanzar
        }

        return true;
    }

    private bool IsNearEdge()
    {
        if (!SelfController.IsGrounded)
            return false;

        Vector2 leftCheck = new Vector2(transform.position.x - lookAheadDistance, transform.position.y);
        Vector2 rightCheck = new Vector2(transform.position.x + lookAheadDistance, transform.position.y);

        bool hasLeftGround = Physics2D.Raycast(leftCheck, Vector2.down, fallCheckDepth, groundLayer).collider != null;
        bool hasRightGround = Physics2D.Raycast(rightCheck, Vector2.down, fallCheckDepth, groundLayer).collider != null;

        return !hasLeftGround || !hasRightGround;
    }

    private bool HasUsefulCard(AIContext ctx)
    {
        if (SelfDeck == null || SelfEnergy == null)
            return false;

        ICardable[] hand = SelfDeck.GetCurrentHand();

        for (int i = 0; i < hand.Length; i++)
        {
            ICardable card = hand[i];

            if (card == null)
                continue;

            if (SelfEnergy.currentEnergy < card.EnergyCost)
                continue;

            if (!card.CanBeUsed(SelfController))
                continue;

            if (card.Type == CardType.Defensive && ctx.inDanger)
                return true;

            if (card.Type == CardType.Offensive && ctx.targetInCardRange)
                return true;

            if (card.Type == CardType.Utility)
                return true;
        }

        return false;
    }

    private bool TryFindCardType(CardType targetType)
    {
        selectedCardIndex = -1;

        if (SelfDeck == null || SelfEnergy == null)
            return false;

        ICardable[] hand = SelfDeck.GetCurrentHand();

        for (int i = 0; i < hand.Length; i++)
        {
            ICardable card = hand[i];

            if (card == null)
                continue;

            if (SelfEnergy.currentEnergy < card.EnergyCost)
                continue;

            if (!card.CanBeUsed(SelfController))
                continue;

            if (card.Type == targetType)
            {
                selectedCardIndex = i;
                return true;
            }
        }

        return false;
    }

    private bool HasEmptyHand()
    {
        if (SelfDeck == null)
            return true;

        ICardable[] hand = SelfDeck.GetCurrentHand();

        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] != null)
                return false;
        }

        return true;
    }

    private void PressCardButton(int index)
    {
        if (index == 0) HasBufferedHand1 = true;
        else if (index == 1) HasBufferedHand2 = true;
        else if (index == 2) HasBufferedHand3 = true;
        else if (index == 3) HasBufferedHand4 = true;
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeGrab() => HasBufferedGrab = false;
    public void ConsumeParry() => HasBufferedParry = false;
    public void ConsumeShield() => HasBufferedShield = false;
    public void ConsumeEvade() => HasBufferedEvade = false;
    public void ConsumeDash() => HasBufferedDash = false;
    public void ConsumeHeavyAttack() => HasBufferedHeavyAttack = false;
    public void ConsumeHeavyAttackRelease() => WasHeavyAttackReleased = false;
    public void ConsumeDrawCards() => HasBufferedDrawCards = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;
    public void ConsumeHand3() => HasBufferedHand3 = false;
    public void ConsumeHand4() => HasBufferedHand4 = false;

    public void SetHeavyAttackHeld(bool held)
    {
        if (held == IsHeavyAttackHeld)
            return;

        IsHeavyAttackHeld = held;
        if (held)
        {
            HasBufferedHeavyAttack = true;
            WasHeavyAttackReleased = false;
        }
        else
        {
            WasHeavyAttackReleased = true;
        }
    }

    public void ClearAllInputs()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeGrab();
        ConsumeParry();
        ConsumeShield();
        ConsumeEvade();
        ConsumeDash();
        ConsumeHeavyAttack();
        ConsumeHeavyAttackRelease();
        IsShieldHeld = false;
        IsHeavyAttackHeld = false;
        ConsumeDrawCards();
        ConsumeHand1();
        ConsumeHand2();
        ConsumeHand3();
        ConsumeHand4();

        CurrentDirection = Vector2.zero;
    }
}

