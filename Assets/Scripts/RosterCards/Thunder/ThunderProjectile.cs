using UnityEngine;

public class ThunderProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage = 15;
    [SerializeField] private Vector2 knockback = new Vector2(0f, 10f);
    [SerializeField] private float lifeTime = 0.5f;

    private GameObject caster;

    public void Init(GameObject casterObject)
    {
        caster = casterObject;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == caster) return;

        if (collision.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(damage, knockback);
        }
    }
}
