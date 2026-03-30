using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int currentDamage = 0;
    public int maxDamage = 100;

    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        if (isDead) return;

        currentDamage += amount;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        GetComponent<PlayerController>().TakeHit(0.4f);

        if (currentDamage >= maxDamage) Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ¡KO! you win");

        gameObject.SetActive(false);
    }
}