using System.Collections;
using UnityEngine;

public class ShadowProjectile : MonoBehaviour
{
    // Matches the first fully extended frame of ShadowSpike.anim.
    [SerializeField, Min(0f)] private float impactTime = 0.45f;
    [SerializeField, Min(0f)] private float lifeTime = 0.85f;
    [SerializeField, Min(0f)] private float hitHalfWidth = 1.2f;
    [SerializeField, Min(0f)] private float groundTolerance = 0.35f;

    private SpriteRenderer spriteRenderer;
    private AIDangerSource dangerSource;
    private CharacterCoordinator caster;
    private CharacterCoordinator target;
    private CharacterMovement slowedMovement;
    private Vector2 groundPosition;
    private int damage;
    private float slowAmount;
    private float slowDuration;
    private Vector2 knockback;
    private float hitStun;
    private KnockbackProfile launch;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(CharacterCoordinator source, CharacterCoordinator markedTarget, Vector2 position,
        int hitDamage, float movementMultiplier, float effectDuration, Vector2 hitKnockback,
        float stun, KnockbackProfile knockbackProfile)
    {
        caster = source;
        target = markedTarget;
        groundPosition = position;
        damage = hitDamage;
        slowAmount = Mathf.Clamp01(movementMultiplier);
        slowDuration = Mathf.Max(0f, effectDuration);
        knockback = hitKnockback;
        hitStun = stun;
        launch = knockbackProfile;
        if (caster != null)
        {
            AIDangerSource.Attach(gameObject, caster.gameObject, hitHalfWidth);
            dangerSource = GetComponent<AIDangerSource>();
        }
        StartCoroutine(SpikeRoutine());
    }

    private IEnumerator SpikeRoutine()
    {
        yield return new WaitForSeconds(impactTime);
        if (dangerSource != null)
            dangerSource.enabled = false;

        if (target != null && target.IsGrounded && target.Health != null &&
            !target.Health.IsDamageImmune && target.Health.Phase == RespawnPhase.Active &&
            Mathf.Abs(target.transform.position.x - groundPosition.x) <= hitHalfWidth)
        {
            Collider2D body = target.GetComponent<Collider2D>();
            if (body != null && Mathf.Abs(body.bounds.min.y - groundPosition.y) <= groundTolerance)
            {
                int attackerIndex = caster != null ? caster.PlayerIndex : -1;
                bool applied = target.Health.ReceiveHit(new CombatHit(damage, knockback, hitStun,
                    HitReaction.Hit, target.transform.position, attackerIndex, launch, attacker: caster));
                if (applied)
                {
                    GetComponent<MatchCardUse>()?.MarkHit();
                    slowedMovement = target.Movement;
                    if (slowedMovement != null && slowDuration > 0f)
                        slowedMovement.SetExternalMoveSpeedMultiplier(this, slowAmount);
                }
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0f, lifeTime - impactTime));
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        yield return new WaitForSeconds(Mathf.Max(0f, slowDuration - (lifeTime - impactTime)));
        Destroy(gameObject);
    }

    // The source frames have different heights and centered pivots. Keep their lower edge on the floor.
    private void LateUpdate()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector3 position = transform.position;
            position.x = groundPosition.x;
            position.y = groundPosition.y - spriteRenderer.sprite.bounds.min.y;
            transform.position = position;
        }
    }

    private void Update()
    {
        if (slowedMovement != null &&
            (target == null || target.Health == null || target.Health.Phase != RespawnPhase.Active))
        {
            slowedMovement.ClearExternalMoveSpeedMultiplier(this);
            slowedMovement = null;
        }
    }

    private void OnDisable()
    {
        if (slowedMovement != null)
            slowedMovement.ClearExternalMoveSpeedMultiplier(this);
    }
}
