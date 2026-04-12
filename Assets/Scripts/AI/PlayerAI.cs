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

    private enum AITactic { Aggressive, Defensive, Zoning }
    private AITactic currentTactic = AITactic.Aggressive;

    private float tacticTimer;
    private float nextAttackTime;
    private float nextCardTime;

    private PlayerController selfController;
    private Transform target;
    private float thinkTimer;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedHand1 { get; private set; }
    public bool HasBufferedHand2 { get; private set; }

    private void Awake() => selfController = GetComponent<PlayerController>();

    private void Start() => Invoke(nameof(FindTarget), 0.5f);

    private void FindTarget()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
        {
            if (p != selfController) { target = p.transform; break; }
        }
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null) { CurrentDirection = Vector2.zero; return; }
        }

        if (selfController.IsDead || selfController.stunTimer > 0)
        {
            CurrentDirection = Vector2.zero;
            return;
        }

        thinkTimer -= Time.deltaTime;
        if (thinkTimer > 0) return;
        thinkTimer = reactionTime;

        ClearAllInputs();

        tacticTimer -= thinkTimer;
        if (tacticTimer <= 0)
        {
            NewTactic();
        }

        float distX = target.position.x - transform.position.x;
        float distY = target.position.y - transform.position.y;
        float absDistX = Mathf.Abs(distX);

        if (distY > 1.5f && selfController.IsGrounded) HasBufferedJump = true;

        switch (currentTactic)
        {
            case AITactic.Aggressive:
                ExecuteAggressive(distX, absDistX, distY);
                break;

            case AITactic.Defensive:
                ExecuteDefensive(distX, absDistX);
                break;

            case AITactic.Zoning:
                ExecuteZoning(distX, absDistX);
                break;
        }
    }

    private bool IsSafeToMove(float directionX)
    {
        if (!selfController.IsGrounded) return true;

        Vector2 rayOrigin = new Vector2(transform.position.x + (directionX * lookAheadDistance), transform.position.y);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, fallCheckDepth, groundLayer);

        Debug.DrawRay(rayOrigin, Vector2.down * fallCheckDepth, hit.collider != null ? Color.green : Color.red, reactionTime);

        return hit.collider != null;
    }

    private void ExecuteAggressive(float distX, float absDistX, float distY)
    {
        if (absDistX > attackRange)
        {
            float dirX = Mathf.Sign(distX);

            if (IsSafeToMove(dirX)) CurrentDirection = new Vector2(dirX, 0);
            else CurrentDirection = Vector2.zero;
        }
        else
        {
            CurrentDirection = Vector2.zero;
            if (Time.time >= nextAttackTime && Mathf.Abs(distY) < 1f)
            {
                HasBufferedAttack = true;
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void ExecuteDefensive(float distX, float absDistX)
    {
        float dirX = -Mathf.Sign(distX);

        if (absDistX > cardRange)
        {
            CurrentDirection = Vector2.zero;
        }
        else
        {
            if (IsSafeToMove(dirX)) CurrentDirection = new Vector2(dirX, 0);
            else CurrentDirection = Vector2.zero;
        }
    }

    private void ExecuteZoning(float distX, float absDistX)
    {
        CurrentDirection = Vector2.zero;

        if (Time.time >= nextCardTime)
        {
            if (Random.value < 0.5f) HasBufferedHand1 = true;
            else HasBufferedHand2 = true;

            nextCardTime = Time.time + timeBetweenCards;
            if (Random.value > 0.5f) NewTactic();
        }
    }

    private void NewTactic()
    {
        float random = Random.value;

        if (random < 0.4f)
        {
            currentTactic = AITactic.Aggressive;
            tacticTimer = 2.0f;
        }
        else if (random < 0.7f)
        {
            currentTactic = AITactic.Zoning;
            tacticTimer = 4.0f;
        }
        else
        {
            currentTactic = AITactic.Defensive;
            tacticTimer = 1.5f;
        }
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeHand1() => HasBufferedHand1 = false;
    public void ConsumeHand2() => HasBufferedHand2 = false;

    public void ClearAllInputs()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeHand1();
        ConsumeHand2();
        CurrentDirection = Vector2.zero;
    }
}
