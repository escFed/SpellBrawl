using UnityEngine;

public class CharacterHealth : MonoBehaviour, IHitStunned
{
    [Header("Health Settings")]
    public int currentDamage = 0;

    [Header("Penalty Settings")]
    public int fallLives = 3;

    public float activeDefenseMultiplier = 1f;

    private Rigidbody2D rb;
    private PlayerController controller;
    private CharacterDeck deck;
    private bool isDead = false;
    private float lastFallTime = -2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        deck = GetComponent<CharacterDeck>();
    }

    public void TakeDamage(int amount, Vector2 baseKnockback)
    {
        TakeDamage(amount, baseKnockback, 0.3f, HitReaction.Hit, transform.position);
    }

    public void TakeDamage(int amount, Vector2 baseKnockback, float hitStun, HitReaction reaction, Vector2 hitPoint)
    {
        if (isDead) return;


        if (controller != null && controller.IsIntangible) return;

        if (controller != null && controller.IsParrying)
        {
            GetComponent<CharacterParry>().OnSuccessfulParry();
            return;
        }

        int finalDamage = Mathf.RoundToInt(amount * controller.stats.defenseMultiplier * activeDefenseMultiplier);

        if (controller != null && controller.Shield != null)
            finalDamage = controller.Shield.AbsorbDamage(finalDamage);

        if (finalDamage <= 0)
            return;

        currentDamage += finalDamage;
        UpdateUI();

        float damageScale = 1f;
        if (currentDamage > 100)
        {
            float extraDamage = currentDamage - 100f;
            damageScale += (extraDamage / 100f) * controller.stats.knockbackMultiplier;
        }

        float weightFactor = controller.stats.weight / 100f;
        Vector2 finalKnockback = (baseKnockback / weightFactor) * damageScale;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(finalKnockback * rb.mass, ForceMode2D.Impulse);

        if (controller != null)
        {
            controller.Combat.TakeHit(Mathf.Max(0f, hitStun), reaction);
            controller.HitFeedback?.Flash(reaction);
        }

        CombatFeedback.PlayImpact(hitPoint, finalKnockback, reaction);
    }

    public void TakePummelDamage(int amount)
    {
        if (isDead || amount <= 0)
            return;

        float defenseMultiplier = controller != null && controller.stats != null ? controller.stats.defenseMultiplier : 1f;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(amount * defenseMultiplier * activeDefenseMultiplier));
        currentDamage += finalDamage;
        UpdateUI();
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

        if (fallLives > 0)
        {
            currentDamage = 0;
            deck?.HandleLifeLost();
            UpdateUI();

            if (RespawnManager.Instance != null && controller != null)
            {
                RespawnManager.Instance.RespawnPlayerAfterFall(this, controller.PlayerIndex);
            }
        }
        else
        {
            UpdateUI();
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
        activeDefenseMultiplier = 1f;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.Shield?.ResetShield();
            controller.Roll?.ResetRolls();
            controller.Dodge?.ResetDodges();
            controller.Dash?.ResetDash();
            Respawn(transform.position);
            deck?.ResetDeckForNewRound();
        }

        EnergyManager energy = GetComponent<EnergyManager>();
        if (energy != null) energy.ResetEnergy();

        if (rb != null) rb.linearVelocity = Vector2.zero;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (controller != null)
        {
            UIEvents.OnDamageChanged?.Invoke(controller.PlayerIndex, currentDamage);
            UIEvents.OnLivesChanged?.Invoke(controller.PlayerIndex, fallLives);
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

    public void OnDeath()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent(out Collider2D col)) col.enabled = false;

        if (GameManager.Instance != null && controller != null)
            GameManager.Instance.PlayerDied(controller.PlayerIndex);
    }

    public void Respawn(Vector3 position)
    {
        isDead = false;

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;

        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = true;
        if (TryGetComponent(out Collider2D col)) col.enabled = true;

        if (controller != null)
        {
            controller.Shield?.ResetShield();
            controller.Roll?.ResetRolls();
            controller.Dodge?.ResetDodges();
            controller.Dash?.ResetDash();
            GetComponent<IInputProvider>()?.ClearAllInputs();
            controller.ChangeState(StateCharacter.Idle);

            controller.Movement.moveSpeedMultiplier = 1f;
            controller.Combat.attackSpeedMultiplier = 1f;
        }
    }
}
