using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int currentDamage = 0;

    [Header("Knockback Settings")]
    public float knockbackMultiplier = 1.5f;

    [Header("Penalty Settings")]
    public int fallLives = 3;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI damageText;
    private GameObject[] lifeIcons;

    private Rigidbody2D rb;
    private bool isDead = false;
    private float lastFallTime = -2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public int PlayerId { get; private set; }

    public void Init(int id)
    {
        PlayerId = id;
    }
    public void SetUIElements(TextMeshProUGUI text, GameObject[] icons)
    {
        damageText = text;
        lifeIcons = icons;
        UpdateUI();
    }

    public void TakeDamage(int amount, Vector2 baseKnockback)
    {
        if (isDead) return;

        currentDamage += amount;
        UpdateUI();

        float damageScale = 1f;

        if (currentDamage > 100)
        {
            float extraDamage = currentDamage - 100f;
            damageScale += (extraDamage / 100f) * knockbackMultiplier;
        }

        Vector2 finalKnockback = baseKnockback * damageScale;

        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(finalKnockback, ForceMode2D.Impulse);

        GetComponent<PlayerController>().TakeHit(0.4f);
    }

    public void FallPenalty()
    {
        if (isDead) return;

        if (Time.time - lastFallTime < 1f) return;
        lastFallTime = Time.time;

        fallLives--;
        UpdateUI();

        if (fallLives > 0)
        {
            PlayerRespawn respawnScript = GetComponent<PlayerRespawn>();
            if (respawnScript != null)
            {
                respawnScript.Respawn();
            }
        }
        else
        {
            Die();
        }
    }

    public void InstantGameOver()
    {
        if (isDead) return;

        fallLives = 0;
        UpdateUI();

        Die();
    }

    private void UpdateUI()
    {
        if (damageText != null)
        {
            damageText.text = currentDamage + "%";
        }

        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].SetActive(i < fallLives);
                }
            }
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

    public int GetPlayerId()
    {
        return PlayerId;
    }
}