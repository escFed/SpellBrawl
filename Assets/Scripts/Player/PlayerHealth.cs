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
    private TextMeshProUGUI damageText;
    private GameObject[] lifeIcons;

    private Rigidbody2D rb;
    private PlayerController controller;
    private bool isDead = false;
    private float lastFallTime = -2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
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

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && controller.IsParrying)
        {
            controller.OnSuccessfulParry();
            return;
        }

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

        if (controller != null) controller.TakeHit(0.4f);
    }

    public void HealDamage(int amount)
    {
        if (isDead) return;

        currentDamage -= amount;

        if (currentDamage < 0)
        {
            currentDamage = 0;
        }

        UpdateUI();
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
            if (RespawnManager.Instance != null && controller != null)
            {
                RespawnManager.Instance.RespawnPlayerAfterFall(this, controller.PlayerIndex);
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

    public void ResetHealth()
    {
        isDead = false;
        currentDamage = 0;
        fallLives = 3;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.Respawn(transform.position);
            controller.ResetDeckForNewRound();
        }

        EnergyManager energy = GetComponent<EnergyManager>();
        if (energy != null) energy.ResetEnergy();

        if (rb != null) rb.linearVelocity = Vector2.zero;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (damageText != null) damageText.text = currentDamage + "%";

        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null) lifeIcons[i].SetActive(i < fallLives);
            }
        }
    }

    private void Die()
    {
        isDead = true;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.EnterDieState();
        }
    }
}