using UnityEngine;

public class StarProjectile : MonoBehaviour
{
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 4f };
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [Header("Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(5f, 5f);
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float speed = 7f;

    private GameObject caster;
    private Transform target;
    private int attackerPlayerIndex = -1;

    public void Init(GameObject casterObject, Transform targetTransform)
    {
        caster = casterObject;
        target = targetTransform;
        attackerPlayerIndex = caster != null && caster.TryGetComponent(out PlayerController controller)
            ? controller.PlayerIndex
            : -1;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (caster != null && collision.transform.root == caster.transform.root) return;

        ICombatHitReceiver hitTarget = collision.GetComponentInParent<ICombatHitReceiver>();
        if (hitTarget != null)
        {
            hitTarget.ReceiveHit(new CombatHit(damage, knockback, hitStun,
                HitReaction.Hit, collision.ClosestPoint(transform.position), attackerPlayerIndex, launch));

            Destroy(gameObject);
        }
    }
}
