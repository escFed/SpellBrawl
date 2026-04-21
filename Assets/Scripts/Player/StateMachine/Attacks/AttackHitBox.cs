using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;

    [SerializeField] private Vector2 knockback = new Vector2(4f, 2f);

    [Header("Energy Settings")]
    [SerializeField] private int energyGain = 10;

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
            float duration = 2f;
            int newDamage = DamageManager.AddGlobalDamageReduction(damage, duration);
            target.TakeDamage(newDamage, knockback);
        }

        EnergyManager Energy = transform.root.GetComponent<EnergyManager>();
        if (Energy != null)
        {
            Energy.AddEnergy(energyGain);
        }
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}

