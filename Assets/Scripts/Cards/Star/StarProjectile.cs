using UnityEngine;

public class StarProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(5f, 5f);
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float speed = 7f;

    private GameObject caster;
    private Transform target;

    public void Init(GameObject casterObject, Transform targetTransform)
    {
        caster = casterObject;
        target = targetTransform;

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

        if (collision.gameObject == caster) return;

        if (collision.TryGetComponent(out IDamageable hitTarget))
        {
            hitTarget.TakeDamage(damage, knockback);
            Destroy(gameObject);
        }
    }
}
