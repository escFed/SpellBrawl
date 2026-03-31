using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(6f, 2f);

    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private GameObject caster;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, GameObject casterObject)
    {
        caster = casterObject;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == caster) return;

        if (collision.TryGetComponent(out IDamageable target))
        {
            float dir = Mathf.Sign(rb.linearVelocity.x);
            target.TakeDamage(damage, new Vector2(knockback.x * dir, knockback.y));

            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}

