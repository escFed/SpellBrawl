using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private AttackStats currentStats;
    private int currentDamage;
    private Vector2 currentKnockback;
    private float currentHitStun;
    private HitReaction currentHitReaction;
    private Collider2D hitCollider;
    private bool hasHit;

    private void Awake() => hitCollider = GetComponent<Collider2D>();

    public void Setup(NormalAttackStats stats)
    {
        Setup(stats, stats != null ? stats.damage : 0, stats != null ? stats.knockback : Vector2.zero);
    }

    public void Setup(AttackStats stats, int damage, Vector2 knockback)
    {
        Setup(stats, damage, knockback, stats != null ? stats.hitStun : 0f);
    }

    public void Setup(AttackStats stats, int damage, Vector2 knockback, float hitStun)
    {
        currentStats = stats;
        currentDamage = Mathf.Max(0, damage);
        currentKnockback = knockback;
        currentHitStun = Mathf.Max(0f, hitStun);
        currentHitReaction = stats != null ? stats.hitReaction : HitReaction.Hit;
        hasHit = false;
    }

    public void BeginSwing() => hitCollider.enabled = true;
    public void EndSwing() => hitCollider.enabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || currentStats == null)
            return;

        if (other.transform.root == transform.root)
            return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null)
            return;

        if (target is CharacterHealth health && health.IsRespawnProtected)
            return;

        float attackerDirection = Mathf.Sign(transform.root.localScale.x);
        if (attackerDirection == 0f)
            attackerDirection = 1f;

        Vector2 directedKnockback = new Vector2(currentKnockback.x * attackerDirection, currentKnockback.y);

        if (target is IHitStunned hitStunTarget)
        {
            Vector2 hitPoint = other.ClosestPoint(hitCollider.bounds.center);
            hitStunTarget.TakeDamage(currentDamage, directedKnockback, currentHitStun, currentHitReaction, hitPoint);
        }
        else
            target.TakeDamage(currentDamage, directedKnockback);
        transform.root.GetComponent<EnergyManager>()?.AddEnergy(currentStats.energyGain);
        hasHit = true;
    }

    private void OnDrawGizmos()
    {
        Collider2D colliderToDraw = GetComponent<Collider2D>();

        if (colliderToDraw != null && colliderToDraw.enabled)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(colliderToDraw.bounds.center, colliderToDraw.bounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(colliderToDraw.bounds.center, colliderToDraw.bounds.size);
        }
    }
}
