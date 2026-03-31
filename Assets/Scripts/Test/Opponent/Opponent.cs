using UnityEngine;

public class Opponent : MonoBehaviour, IDamageable
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
        Debug.Log($"¡opponent damage {amount}! Total: {currentDamage}%");

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        if (currentDamage >= maxDamage)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Opponent Defeat You Win");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Destroy(gameObject);
    }
}