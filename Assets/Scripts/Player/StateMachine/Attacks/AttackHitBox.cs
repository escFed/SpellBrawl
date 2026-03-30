using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    [SerializeField] private Vector2 knockback = new Vector2(4f, 2f);

    private readonly HashSet<Collider2D> _hitThisSwing = new();

    public void BeginSwing()
    {
        _hitThisSwing.Clear();
        gameObject.SetActive(true);
    }

    public void EndSwing() => gameObject.SetActive(false);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root == transform.root) return;

        if (_hitThisSwing.Contains(other)) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            _hitThisSwing.Add(other);

            float dir = Mathf.Sign(other.transform.position.x - transform.root.position.x);
            target.TakeDamage(damage, new Vector2(knockback.x * dir, knockback.y));
        }
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
