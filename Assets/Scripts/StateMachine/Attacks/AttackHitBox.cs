using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    public AttackStats attackData;

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
            target.TakeDamage(attackData.damage, new Vector2(attackData.knockback.x * dir, attackData.knockback.y));

            EnergyManager Energy = transform.root.GetComponent<EnergyManager>();
            if (Energy != null)
            {
                Energy.AddEnergy(attackData.energyGain);
            }
        }
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
