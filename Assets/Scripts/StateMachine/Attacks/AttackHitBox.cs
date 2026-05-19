using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private AttackStats currentStats;
    private Collider2D Collider;
    private bool hasHit = false;

    private void Awake() => Collider = GetComponent<Collider2D>();

    public void Setup(AttackStats stats)
    {
        currentStats = stats;
        hasHit = false;
    }

    public void BeginSwing() => Collider.enabled = true;
    public void EndSwing() => Collider.enabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || currentStats == null) return;

        if (other.transform.root == transform.root) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(currentStats.damage, currentStats.knockback);
            transform.root.GetComponent<EnergyManager>()?.AddEnergy(currentStats.energyGain);
            hasHit = true;
        }
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