using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour, IHitStunned
{
    private const float RespawnBlinkInterval = 0.1f;

    [Header("Health Settings")]
    public int currentDamage = 0;

    [Header("Penalty Settings")]
    public int fallLives = 3;

    public float activeDefenseMultiplier = 1f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private SpriteRenderer sprite;
    private PlayerController controller;
    private CharacterDeck deck;
    private bool isDead = false;
    private bool isWaitingToRespawn;
    private float lastFallTime = -2f;
    private Coroutine respawnRoutine;

    public bool IsRespawnProtected { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        controller = GetComponent<PlayerController>();
        deck = GetComponent<CharacterDeck>();
    }

    public void TakeDamage(int amount, Vector2 baseKnockback)
    {
        TakeDamage(amount, baseKnockback, 0.3f, HitReaction.Hit, transform.position);
    }

    public void TakeDamage(int amount, Vector2 baseKnockback, float hitStun, HitReaction reaction, Vector2 hitPoint)
    {
        if (isDead || IsRespawnProtected) return;


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
        if (isDead || IsRespawnProtected || amount <= 0)
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

        if (isDead || isWaitingToRespawn) return;

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
        CancelRespawnSequence();
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
        CancelRespawnSequence();
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

        SetCharacterVisible(false);
        if (bodyCollider != null) bodyCollider.enabled = false;

        if (GameManager.Instance != null && controller != null)
            GameManager.Instance.PlayerDied(controller.PlayerIndex);
    }

    public void Respawn(Vector3 position)
    {
        isDead = false;

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;

        SetCharacterVisible(true);
        if (bodyCollider != null) bodyCollider.enabled = true;

        if (controller != null)
        {
            GetComponent<IInputProvider>()?.ClearAllInputs();
            controller.ChangeState(StateCharacter.Idle);
            controller.ResetJumps();

            controller.Movement.moveSpeedMultiplier = 1f;
            controller.Combat.attackSpeedMultiplier = 1f;
        }
    }

    public void BeginRespawnSequence(Vector3 position, float delay, float protectionDuration)
    {
        CancelRespawnSequence();
        respawnRoutine = StartCoroutine(RespawnSequence(position, Mathf.Max(0f, delay), Mathf.Max(0f, protectionDuration)));
    }

    public void CancelRespawnProtection()
    {
        if (!IsRespawnProtected)
            return;

        IsRespawnProtected = false;
        SetCharacterVisible(true);
    }

    private IEnumerator RespawnSequence(Vector3 position, float delay, float protectionDuration)
    {
        isWaitingToRespawn = true;
        PrepareForRespawnDelay(position);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (isDead)
        {
            respawnRoutine = null;
            yield break;
        }

        Respawn(position);
        isWaitingToRespawn = false;

        if (controller != null)
        {
            controller.ActiveInput?.ClearAllInputs();
            controller.controlsEnabled = true;
        }

        IsRespawnProtected = protectionDuration > 0f;
        float elapsed = 0f;
        float blinkTimer = 0f;

        while (IsRespawnProtected && elapsed < protectionDuration)
        {
            elapsed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= RespawnBlinkInterval)
            {
                blinkTimer -= RespawnBlinkInterval;
                SetCharacterVisible(sprite == null || !sprite.enabled);
            }

            yield return null;
        }

        CancelRespawnProtection();
        respawnRoutine = null;
    }

    private void PrepareForRespawnDelay(Vector3 position)
    {
        controller?.Grab?.ReleaseGrabbedTarget();

        if (controller != null)
        {
            controller.controlsEnabled = false;
            controller.ActiveInput?.ClearAllInputs();
            controller.ChangeState(StateCharacter.Idle);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        transform.position = position;
        SetCharacterVisible(false);

        if (bodyCollider != null)
            bodyCollider.enabled = false;
    }

    private void CancelRespawnSequence()
    {
        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
        }

        isWaitingToRespawn = false;
        CancelRespawnProtection();
    }

    private void SetCharacterVisible(bool visible)
    {
        if (sprite != null)
            sprite.enabled = visible;
    }
}
