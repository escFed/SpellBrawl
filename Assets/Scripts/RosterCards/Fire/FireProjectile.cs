using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 3.5f };
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(6f, 2f);

    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private GameObject caster;
    private int attackerPlayerIndex = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, GameObject casterObject)
    {
        caster = casterObject;
        attackerPlayerIndex = caster != null && caster.TryGetComponent(out PlayerController controller)
            ? controller.PlayerIndex
            : -1;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (caster != null && collision.transform.root == caster.transform.root) return;

        ICombatHitReceiver target = collision.GetComponentInParent<ICombatHitReceiver>();
        if (target != null)
        {
            float dir = Mathf.Sign(rb.linearVelocity.x);
            Vector2 directedKnockback = new Vector2(knockback.x * dir, knockback.y);

            target.ReceiveHit(new CombatHit(damage, directedKnockback, hitStun,
                HitReaction.Hit, collision.ClosestPoint(transform.position), attackerPlayerIndex, launch));

            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}

