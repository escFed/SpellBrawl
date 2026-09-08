using UnityEngine;

public class ThunderProjectile : MonoBehaviour
{
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 7f };
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [Header("Stats")]
    [SerializeField] private int damage = 15;
    [SerializeField] private Vector2 knockback = new Vector2(0f, 10f);
    [SerializeField] private float lifeTime = 0.5f;

    private GameObject caster;
    private int attackerPlayerIndex = -1;

    public void Init(GameObject casterObject)
    {
        caster = casterObject;
        attackerPlayerIndex = caster != null && caster.TryGetComponent(out PlayerController controller)
            ? controller.PlayerIndex
            : -1;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (caster != null && collision.transform.root == caster.transform.root) return;

        ICombatHitReceiver target = collision.GetComponentInParent<ICombatHitReceiver>();
        if (target != null)
        {
            target.ReceiveHit(new CombatHit(damage, knockback, hitStun, HitReaction.Hit, collision.ClosestPoint(transform.position), attackerPlayerIndex, launch));
        }
    }
}
