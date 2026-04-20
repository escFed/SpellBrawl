using UnityEngine;

public class PlayerAI : MonoBehaviour, IInputProvider
{
    [Header("Settings")]
    public float attackRange = 1.2f;
    public float cardRange = 6f;
    public float reactionTime = 0.1f;

    [Header("Behaviours")]
    public float attackCooldown = 0.5f;
    public float timeBetweenCards = 2.5f;

    [Header("Fall check")]
    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 2f;

    private AIBehavior currentBehavior;
    public OffensiveBehavior offensiveBehavior;
    public DefensiveBehavior defensiveBehavior;
    public PatrolBehavior patrolBehavior;
    public PlayerController SelfController { get; private set; }
    public Transform Target { get; private set; }

    private float thinkTimer;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }
    public bool HasBufferedHand3 { get; private set; }
    public bool HasBufferedHand4 { get; private set; }
    public bool HasBufferedHand5 { get; private set; }

    private void Awake()
    {
        SelfController = GetComponent<PlayerController>();

        offensiveBehavior = new OffensiveBehavior();
        defensiveBehavior = new DefensiveBehavior();
        patrolBehavior = new PatrolBehavior();
    }

    private void Start()
    {
        Invoke(nameof(FindTarget), 0.5f);
        ChangeBehavior(offensiveBehavior);
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
        if (Target == null)
        {
            FindTarget();
            if (Target == null) { CurrentDirection = Vector2.zero; return; }
        }

        if (SelfController.IsDead || SelfController.stunTimer > 0)
        {
            CurrentDirection = Vector2.zero;
            return;
        }

        thinkTimer -= Time.deltaTime;
        if (thinkTimer > 0) return;
        thinkTimer = reactionTime;

        ClearAllInputs();

        if (currentBehavior != null)
        {
            currentBehavior.UpdateBehavior(this);
        }
    }

    public void ChangeBehavior(AIBehavior newBehavior)
    {
        if (currentBehavior != null) currentBehavior.Exit(this);
        currentBehavior = newBehavior;
        if (currentBehavior != null) currentBehavior.Enter(this);
    }

    public void SetDirection(Vector2 dir) => CurrentDirection = dir;
    public void TriggerJump() => HasBufferedJump = true;
    public void TriggerAttack() => HasBufferedAttack = true;
    public void TriggerHand1() => HasBufferedHand1 = true;
    public void TriggerHand2() => HasBufferedHand2 = true;

    public bool IsSafeToMove(float directionX)
    {
        if (!SelfController.IsGrounded) return true;

        Vector2 rayOrigin = new Vector2(transform.position.x + (directionX * lookAheadDistance), transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, fallCheckDepth, groundLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * fallCheckDepth, hit.collider != null ? Color.green : Color.red, reactionTime);

        return hit.collider != null;
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;
    public void ConsumeHand3() => HasBufferedHand3 = false;
    public void ConsumeHand4() => HasBufferedHand4 = false;
    public void ConsumeHand5() => HasBufferedHand5 = false;

    public void ClearAllInputs()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeHand1();
        ConsumeHand2();
        ConsumeHand3();
        ConsumeHand4();
        ConsumeHand5();
        CurrentDirection = Vector2.zero;
    }
}