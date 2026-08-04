using System;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable, IGrabbable
{
    [Header("Training Dummy")]
    [SerializeField] private int currentDamage;
    [SerializeField] private int maximumDisplayedDamage = 9999;

    [Header("Physics")]
    [SerializeField, Min(1f)] private float weight = 100f;
    [SerializeField, Min(0f)] private float knockbackMultiplier = 1f;
    [SerializeField, Min(0f)] private float gravityScale = 1f;

    public event Action<int> DamageChanged;

    public int CurrentDamage => currentDamage;
    public bool CanBeGrabbed => enabled && gameObject.activeInHierarchy && !isGrabbed;
    public Transform GrabTransform => transform;

    private Rigidbody2D body;
    private Vector3 spawnPosition;
    private bool configured;
    private bool isGrabbed;
    private RigidbodyType2D bodyTypeBeforeGrab;
    private float gravityScaleBeforeGrab;
    private bool physicsSuspended;
    private bool stopHorizontalKnockbackOnLanding;

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

    private void FixedUpdate()
    {
        if (body == null || isGrabbed || stopHorizontalKnockbackOnLanding)
            return;

        body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
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

    public void TakeDamage(int amount, Vector2 knockback)
    {
        AddDamage(amount);

        if (!isGrabbed)
        {
            stopHorizontalKnockbackOnLanding = Mathf.Abs(knockback.x) > 0.01f;
            ApplyKnockback(knockback);
        }
    }

    public void OnGrabbed(Transform holdPoint)
    {
        isGrabbed = true;
        stopHorizontalKnockbackOnLanding = false;
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
        {
            transform.position = holdPoint.position;
        }
    }

    public void TakePummelDamage(int amount)
    {
        AddDamage(amount);
    }

    public void OnThrown(int amount, Vector2 knockback)
    {
        AddDamage(amount);
        isGrabbed = false;
        stopHorizontalKnockbackOnLanding = Mathf.Abs(knockback.x) > 0.01f;
        RestorePhysics();
        ApplyKnockback(knockback);
    }

    public void OnReleased()
    {
        isGrabbed = false;
        stopHorizontalKnockbackOnLanding = false;
        RestorePhysics();
    }

    public void ResetDummy()
    {
        currentDamage = 0;
        isGrabbed = false;
        stopHorizontalKnockbackOnLanding = false;
        RestorePhysics();
        ConfigurePhysics();
        DamageChanged?.Invoke(currentDamage);
        ReturnToSpawn();
    }

    private void AddDamage(int amount)
    {
        currentDamage = Mathf.Clamp(currentDamage + Mathf.Max(0, amount), 0, Mathf.Max(1, maximumDisplayedDamage));

        DamageChanged?.Invoke(currentDamage);
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

    private void ApplyKnockback(Vector2 baseKnockback)
    {
        if (body == null || body.bodyType != RigidbodyType2D.Dynamic)
            return;

        float damageScale = 1f;
        if (currentDamage > 100)
        {
            float extraDamage = currentDamage - 100f;
            damageScale += (extraDamage / 100f) * knockbackMultiplier;
        }

        float weightFactor = weight / 100f;
        Vector2 finalKnockback = (baseKnockback / weightFactor) * damageScale;

        StopBody();
        body.AddForce(finalKnockback * body.mass, ForceMode2D.Impulse);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopKnockbackMovementOnGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        StopKnockbackMovementOnGroundContact(collision);
    }

    private void StopKnockbackMovementOnGroundContact(Collision2D collision)
    {
        if (!stopHorizontalKnockbackOnLanding || body == null || body.linearVelocity.y > 0.01f)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < 0.5f)
                continue;

            StopBody();
            stopHorizontalKnockbackOnLanding = false;
            return;
        }
    }

    private void ReturnToSpawn()
    {
        if (configured)
        {
            if (body != null)
                body.position = spawnPosition;
            else
                transform.position = spawnPosition;
        }

        StopBody();
    }

    private void StopBody()
    {
        if (body == null)
            return;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void OnValidate()
    {
        maximumDisplayedDamage = Mathf.Max(1, maximumDisplayedDamage);
        weight = Mathf.Max(1f, weight);
        knockbackMultiplier = Mathf.Max(0f, knockbackMultiplier);
        gravityScale = Mathf.Max(0f, gravityScale);

        if (GetComponent<Collider2D>() == null)
            Debug.LogWarning("TrainingDummy needs a Collider2D to receive hits and grabs.", this);
    }
}
