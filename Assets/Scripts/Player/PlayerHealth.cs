using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int currentDamage = 0;
    public int maxDamage = 100;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI damageText;

    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDamageText(TextMeshProUGUI text)
    {
        damageText = text;
        UpdateUI();
    }

    public void TakeDamage(int amount, Vector2 knockback)
    {
        if (isDead) return;

        currentDamage += amount;
        UpdateUI();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        GetComponent<PlayerController>().TakeHit(0.4f);

        if (currentDamage >= maxDamage) Die();
    }

    private void UpdateUI()
    {
        if (damageText != null)
        {
            damageText.text = currentDamage + "%";
        }
    }

    private void Die()
    {
        isDead = true;

        gameObject.SetActive(false);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && MatchManager.Instance != null)
        {
            MatchManager.Instance.PlayerDied(controller.PlayerIndex);
        }
    }
}