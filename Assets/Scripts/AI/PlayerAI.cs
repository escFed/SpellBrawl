using UnityEngine;

public class PlayerAI : MonoBehaviour, IInputProvider
{
    [Header("IA Settings")]
    public float attackRange = 1.2f;
    public float cardRange = 6f;
    public float reactionTime = 0.2f;
    public float attackCooldown = 1.2f;
    private float nextAttackTime = 0f;
    private PlayerController selfController;
    private Transform target;
    private float thinkTimer;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedSpecial { get; private set; }

    private void Awake()
    {
        selfController = GetComponent<PlayerController>();
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
            if (p != selfController)
            {
                target = p.transform;
                break;
            }
        }
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null)
            {
                CurrentDirection = Vector2.zero;
                return;
            }
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

        float distX = target.position.x - transform.position.x;
        float distY = target.position.y - transform.position.y;
        float absDistX = Mathf.Abs(distX);

        if (distY > 1.5f && selfController.IsGrounded) HasBufferedJump = true;

        if (absDistX <= attackRange && Mathf.Abs(distY) < 1f)
        {
            CurrentDirection = Vector2.zero;
            if (Time.time >= nextAttackTime)
            {
                HasBufferedAttack = true;
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            CurrentDirection = new Vector2(Mathf.Sign(distX), 0);

            if (absDistX > attackRange && absDistX < cardRange && Random.value < 0.6f)
            {
                HasBufferedSpecial = true;

                int randomDirection = Random.Range(0, 4);
                if (randomDirection == 0) CurrentDirection = Vector2.zero;
                else if (randomDirection == 1) CurrentDirection = new Vector2(Mathf.Sign(distX), 0);
                else if (randomDirection == 2) CurrentDirection = Vector2.up;
                else CurrentDirection = Vector2.down;
            }
        }
    }

    public void ConsumeJump() => HasBufferedJump = false;
    public void ConsumeAttack() => HasBufferedAttack = false;
    public void ConsumeSpecial() => HasBufferedSpecial = false;

    public void ClearAllInputs()
    {
        ConsumeJump();
        ConsumeAttack();
        ConsumeSpecial();
        CurrentDirection = Vector2.zero;
    }
}
