using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private AttackStats currentStats;
    private Collider2D hitCollider;
    private bool hasHit;

    private void Awake() => hitCollider = GetComponent<Collider2D>();

    public void Setup(AttackStats stats)
    {
        currentStats = stats;
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

        float attackerDirection = Mathf.Sign(transform.root.localScale.x);
        if (attackerDirection == 0f)
            attackerDirection = 1f;

        Vector2 directedKnockback = new Vector2(currentStats.knockback.x * attackerDirection, currentStats.knockback.y);

        target.TakeDamage(currentStats.damage, directedKnockback);
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