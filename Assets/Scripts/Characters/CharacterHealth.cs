using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour, ICombatHitReceiver
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

    public bool ReceiveHit(CombatHit hit)
    {
        if (isDead || IsRespawnProtected || controller.IsIntangible) return false;

        if (controller != null && controller.IsParrying)
        {
            GetComponent<CharacterParry>().OnSuccessfulParry();
            return false;
        }

        // A zero-damage displacement still respects a held shield.
        if (hit.Damage == 0 && controller.Shield != null && controller.Shield.IsActive)
            return false;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(hit.Damage * controller.stats.defenseMultiplier * activeDefenseMultiplier));

        if (controller != null && controller.Shield != null)
            finalDamage = controller.Shield.AbsorbDamage(finalDamage);

        if (finalDamage <= 0 && (hit.Damage > 0 || hit.BaseKnockback.sqrMagnitude < 0.000001f))
            return false;

        currentDamage += finalDamage;
        UpdateUI();

        // Capture held direction before TakeHit clears action buffers.
        Vector2 influence = controller.ActiveInput != null ? controller.ActiveInput.CurrentDirection : Vector2.zero;
        Vector2 finalKnockback = KnockbackCalculation.CalculateVelocity(hit, currentDamage, controller.stats.weight,
            influence, controller.stats.directionalInfluenceDegrees);
        float stun = KnockbackCalculation.CalculateHitStun(hit, finalKnockback);

        // Exit the interrupted state before installing the new launch: exits may stop movement.
        controller.Combat.TakeHit(stun, hit.Reaction);
        controller.Movement.ApplyKnockback(finalKnockback);
        controller.HitFeedback?.Flash(hit.Reaction);

        if (hit.AttackerPlayerIndex >= 0)
            CombatFeedback.PlayImpact(hit.Point, finalKnockback, hit.Reaction, PlayerColors.Get(hit.AttackerPlayerIndex));
        else
            CombatFeedback.PlayImpact(hit.Point, finalKnockback, hit.Reaction);
        return true;
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

        if (!isActiveAndEnabled || isDead || isWaitingToRespawn) return;

        if (Time.time - lastFallTime < 1f) return;
        lastFallTime = Time.time;

        fallLives--;

        controller.Movement.ResetKnockback();
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
        this.controller.Movement.ResetKnockback();
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
        controller.Movement.ResetKnockback();
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
        if (!isActiveAndEnabled || isDead)
            return;

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
