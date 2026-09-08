using System;
using UnityEngine;

public class Dummy : MonoBehaviour, ICombatHitReceiver, IGrabbable
{
    [Header("Training Dummy")]
    [SerializeField] private int currentDamage;
    [SerializeField] private int maximumDisplayedDamage = 9999;
    [Tooltip("Optional character to reproduce. Training defaults to the selected player when unset.")]
    [SerializeField] private CharacterStats targetCharacter;
    [Tooltip("Held defender direction used for repeatable directional-influence tests.")]
    [SerializeField] private Vector2 directionalInput;

    [Header("Fallback Physics")]
    [SerializeField, Min(1f)] private float weight = 100f;
    [SerializeField, Min(0f)] private float gravityScale = 1f;
    [SerializeField, Min(0.01f)] private float airDeceleration = 10f;
    [SerializeField, Min(0.01f)] private float groundDeceleration = 40f;

    public event Action<int> DamageChanged;
    public int CurrentDamage => currentDamage;
    public float HitStunRemaining { get; private set; }
    public Vector2 KnockbackVelocity => motion.Velocity;
    public CharacterStats TargetCharacter => targetCharacter;
    public Vector2 DirectionalInput { get => directionalInput; set => directionalInput = Vector2.ClampMagnitude(value, 1f); }
    public TrainingLaunchTrace Trace { get; } = new TrainingLaunchTrace();
    public bool CanBeGrabbed => enabled && gameObject.activeInHierarchy && !isGrabbed;
    public Transform GrabTransform => transform;

    private readonly KnockbackMotion motion = new KnockbackMotion();
    private Rigidbody2D body;
    private Vector3 spawnPosition;
    private bool configured;
    private bool isGrabbed;
    private RigidbodyType2D bodyTypeBeforeGrab;
    private float gravityScaleBeforeGrab;
    private bool physicsSuspended;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        ConfigurePhysics();
    }

    private void Start()
    {
        if (!configured)
            Configure(transform.position);
    }

    private void Update() => HitStunRemaining = Mathf.Max(0f, HitStunRemaining - Time.deltaTime);

    private void FixedUpdate()
    {
        if (body == null || isGrabbed)
            return;
        Trace.Record(transform.position, Time.fixedDeltaTime, body.linearVelocity.sqrMagnitude > 0.01f || HitStunRemaining > 0f);
        motion.Step(body, Time.fixedDeltaTime,
            targetCharacter != null ? targetCharacter.knockbackAirDeceleration : airDeceleration,
            targetCharacter != null ? targetCharacter.knockbackGroundDeceleration : groundDeceleration);
        // The dummy has neutral movement input, just like a player who releases the stick.
        motion.SetOrdinaryVelocity(body, new Vector2(0f, body.linearVelocity.y - motion.Velocity.y));
    }

    public void UseCharacterStats(CharacterStats stats)
    {
        targetCharacter = stats;
        ResetDummy();
    }

    public void SetDamage(int damage)
    {
        currentDamage = Mathf.Clamp(damage, 0, Mathf.Max(1, maximumDisplayedDamage));
        DamageChanged?.Invoke(currentDamage);
    }

    public void Configure(Vector3 position)
    {
        RestorePhysics();
        spawnPosition = position;
        configured = true;
        isGrabbed = false;
        transform.position = spawnPosition;
        ConfigurePhysics();
    }



    public bool ReceiveHit(CombatHit hit)
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return false;
        int amount = Mathf.Max(0, Mathf.RoundToInt(hit.Damage *
            (targetCharacter != null ? targetCharacter.defenseMultiplier : 1f)));
        if (amount == 0 && (hit.Damage > 0 || hit.BaseKnockback.sqrMagnitude < 0.000001f))
            return false;

        int before = currentDamage;
        SetDamage(currentDamage + amount);
        if (isGrabbed || body == null || body.bodyType != RigidbodyType2D.Dynamic)
            return true;

        Vector2 velocity = KnockbackCalculation.CalculateVelocity(hit, currentDamage,
            targetCharacter != null ? targetCharacter.weight : weight, directionalInput,
            targetCharacter != null ? targetCharacter.directionalInfluenceDegrees : 12f);
        HitStunRemaining = Mathf.Max(HitStunRemaining, KnockbackCalculation.CalculateHitStun(hit, velocity));
        Trace.Begin(transform.position, velocity, HitStunRemaining, before, currentDamage);
        motion.Launch(body, velocity);
        if (hit.AttackerPlayerIndex >= 0)
            CombatFeedback.PlayImpact(hit.Point, velocity, hit.Reaction, PlayerColors.Get(hit.AttackerPlayerIndex));
        else
            CombatFeedback.PlayImpact(hit.Point, velocity, hit.Reaction);
        return true;
    }

    public void OnGrabbed(Transform holdPoint)
    {
        isGrabbed = true;
        HitStunRemaining = 0f;
        SuspendPhysics();
        UpdateGrabbedPosition(holdPoint);
    }

    public void UpdateGrabbedPosition(Transform holdPoint)
    {
        if (!isGrabbed || holdPoint == null)
            return;
        if (body != null)
        {
            body.position = holdPoint.position;
            StopBody();
        }
        else
            transform.position = holdPoint.position;
    }

    public void TakePummelDamage(int amount)
    {
        SetDamage(currentDamage + Mathf.Max(0, Mathf.RoundToInt(amount *
            (targetCharacter != null ? targetCharacter.defenseMultiplier : 1f))));
    }



    public void OnThrown(CombatHit hit)
    {
        isGrabbed = false;
        RestorePhysics();
        ReceiveHit(hit);
    }

    public void OnReleased()
    {
        isGrabbed = false;
        RestorePhysics();
    }

    public void ResetDummy()
    {
        isGrabbed = false;
        HitStunRemaining = 0f;
        RestorePhysics();
        ConfigurePhysics();
        SetDamage(0);
        ReturnToSpawn();
    }

    private void ConfigurePhysics()
    {
        if (body == null)
            return;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = gravityScale;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        StopBody();
    }

    private void SuspendPhysics()
    {
        if (body == null || physicsSuspended)
            return;
        bodyTypeBeforeGrab = body.bodyType;
        gravityScaleBeforeGrab = body.gravityScale;
        physicsSuspended = true;
        StopBody();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    private void RestorePhysics()
    {
        if (body == null || !physicsSuspended)
            return;
        body.bodyType = bodyTypeBeforeGrab;
        body.gravityScale = gravityScaleBeforeGrab;
        StopBody();
        physicsSuspended = false;
    }

    private void ReturnToSpawn()
    {
        if (configured)
        {
            if (body != null)
                body.position = spawnPosition;
            // Keep diagnostics and an immediate repeat hit in sync before the next physics step.
            transform.position = spawnPosition;
        }
        StopBody();
    }

    private void StopBody()
    {
        Trace.Stop();
        motion.Clear(body);
        if (body != null)
            body.angularVelocity = 0f;
    }

    private void OnDisable()
    {
        OnReleased();
        HitStunRemaining = 0f;
        StopBody();
    }

    private void OnValidate()
    {
        maximumDisplayedDamage = Mathf.Max(1, maximumDisplayedDamage);
        weight = Mathf.Max(1f, weight);
        gravityScale = Mathf.Max(0f, gravityScale);
        airDeceleration = Mathf.Max(0.01f, airDeceleration);
        groundDeceleration = Mathf.Max(0.01f, groundDeceleration);
    }
}
