using UnityEngine;

public class Opponent : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int currentDamage = 0;

    public int maxDamage = 100;

    private Rigidbody2D rb;
    private bool isDead = false;

    [SerializeField] private int opponentId;
    public int OpponentId => opponentId;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        if (isDead) return;

        currentDamage += amount;

        // Actualizar UI
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateDamage(opponentId, currentDamage, 0);

        // Aplicar reducción de knockback igual que el jugador
        Vector2 finalKnockback = DamageManager.CalculateKnockback(opponentId, knockback);

        // Escala por daño acumulado (igual que PlayerHealth)
        float damageScale = 1f;
        if (currentDamage > 100)
        {
            float extra = currentDamage - 100f;
            damageScale += (extra / 100f) * 1.5f;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(finalKnockback * damageScale, ForceMode2D.Impulse);

        if (currentDamage >= maxDamage)
            Die();
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        // Avisar al MatchManager para mostrar la victoria
        if (MatchManager.Instance != null)
            MatchManager.Instance.PlayerDied(opponentId);

        Destroy(gameObject);
    }

 

    public int GetPlayerId()
    {
        return opponentId;
    }
}