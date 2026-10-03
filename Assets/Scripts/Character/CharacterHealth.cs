using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour, ICombatHitReceiver
{
    private float RespawnBlinkInterval = 0.1f;

    [Header("Health Settings")]
    public int currentDamage = 0;

    [Header("Penalty Settings")]
    public int fallLives = 3;

    public float activeDefenseMultiplier = 1f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private SpriteRenderer sprite;
    private CharacterCoordinator controller;
    private CharacterParry parry;
    private CharacterDeck deck;
    private float lastFallTime = -2f;
    private Coroutine respawnRoutine;

    public RespawnPhase Phase { get; private set; } = RespawnPhase.Active;
    public bool IsDead => Phase == RespawnPhase.Eliminated;
    public bool IsIntangible { get; private set; }
    public bool IsRespawnProtected => Phase == RespawnPhase.RespawnProtected;
    public bool ShouldCameraTrack => Phase == RespawnPhase.Active || Phase == RespawnPhase.RespawnProtected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        controller = GetComponent<CharacterCoordinator>();
        parry = GetComponent<CharacterParry>();
        deck = GetComponent<CharacterDeck>();
    }

    private void Start()
    {
        UpdateUI();
    }

    public void SetIntangible(bool intangible)
    {
        IsIntangible = intangible;
    }

    public bool ReceiveHit(CombatHit hit)
    {
        if (Phase != RespawnPhase.Active || IsIntangible) return false;

        if (parry != null && parry.IsParrying)
        {
            parry.OnSuccessfulParry(hit.Attacker);
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
        if (controller.ActiveInput is IDirectionalInfluenceProvider influenceProvider)
            influence = influenceProvider.DirectionalInfluence;
        Vector2 finalKnockback = KnockbackCalculation.CalculateVelocity(hit, currentDamage, controller.stats.weight,
            influence, controller.stats.directionalInfluenceDegrees);
        float stun = KnockbackCalculation.CalculateHitStun(hit, finalKnockback);

        // Exit the interrupted state before installing the new launch: exits may stop movement.
        controller.Combat.TakeHit(stun, hit.Reaction);
        controller.Movement.ApplyKnockback(finalKnockback, hit.AirDecelerationMultiplier);
        controller.Movement.BeginAerialSuspension(hit.DefenderSuspension);
        controller.HitFeedback?.Flash(hit.Reaction);

        if (hit.AttackerPlayerIndex >= 0)
            CombatFeedback.PlayImpact(hit.Point, finalKnockback, hit.Reaction, PlayerColors.Get(hit.AttackerPlayerIndex));
        else
            CombatFeedback.PlayImpact(hit.Point, finalKnockback, hit.Reaction);
        return true;
    }

    public void TakePummelDamage(int amount)
    {
        if (Phase != RespawnPhase.Active || amount <= 0)
            return;

        float defenseMultiplier = controller != null && controller.stats != null ? controller.stats.defenseMultiplier : 1f;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(amount * defenseMultiplier * activeDefenseMultiplier));
        currentDamage += finalDamage;
        UpdateUI();
    }

    public void HealDamage(int amount)
    {
        if (IsDead) return;

        currentDamage -= amount;

        if (currentDamage < 0)
        {
            currentDamage = 0;
        }

        UpdateUI();
    }

    public void FallPenalty()
    {
        bool canLoseStock = Phase == RespawnPhase.Active || Phase == RespawnPhase.RespawnProtected;
        if (!isActiveAndEnabled || !canLoseStock) return;

        if (Time.time - lastFallTime < 1f) return;
        lastFallTime = Time.time;

        fallLives--;

        if (fallLives > 0)
        {
            ApplyStockRespawnResetPolicy();
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
        if (IsDead) return;

        fallLives = 0;
        UpdateUI();
        Die();
    }

    public void ResetHealth()
    {
        CancelRespawnSequence();
        ApplyNewRoundResetPolicy();
        Respawn(transform.position);
        SetPhase(RespawnPhase.Active);
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
        ResetTransientCharacterState();
        SetPhase(RespawnPhase.Eliminated);

        CharacterCoordinator controller = GetComponent<CharacterCoordinator>();
        if (controller != null)
        {
            controller.EnterDieState();
        }
    }

    public void OnDeath()
    {
        SetPhase(RespawnPhase.Eliminated);
        IsIntangible = false;

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
        IsIntangible = false;

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;

        SetCharacterVisible(true);
        if (bodyCollider != null) bodyCollider.enabled = true;

        if (controller != null)
        {
            controller.ActiveInput?.ClearAllInputs();
            controller.ChangeState(controller.States.Idle);
            controller.ResetJumps();
        }
    }

    public void BeginRespawnSequence(Vector3 position, float delay, float protectionDuration)
    {
        if (!isActiveAndEnabled || Phase == RespawnPhase.RespawnDelay || IsDead)
            return;

        CancelRespawnSequence();
        SetPhase(RespawnPhase.RespawnDelay);
        respawnRoutine = StartCoroutine(RespawnSequence(position, Mathf.Max(0f, delay), Mathf.Max(0f, protectionDuration)));
    }

    public void CancelRespawnProtection()
    {
        if (!IsRespawnProtected)
            return;

        SetPhase(RespawnPhase.Active);
        SetCharacterVisible(true);
    }

    private IEnumerator RespawnSequence(Vector3 position, float delay, float protectionDuration)
    {
        PrepareForRespawnDelay(position);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (IsDead)
        {
            respawnRoutine = null;
            yield break;
        }

        Respawn(position);

        if (controller != null)
        {
            controller.ActiveInput?.ClearAllInputs();
            controller.SetControlsEnabled(true);
        }

        SetPhase(protectionDuration > 0f ? RespawnPhase.RespawnProtected : RespawnPhase.Active);
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
        ResetTransientCharacterState();

        if (controller != null)
        {
            controller.SetControlsEnabled(false);
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

        if (IsRespawnProtected)
            SetCharacterVisible(true);
    }

    private void ApplyStockRespawnResetPolicy()
    {
        // Stock loss resets transient combat state while preserving strategic resources:
        // energy, deck order/hand, card cooldowns, and movement-ability cooldowns.
        currentDamage = 0;
        activeDefenseMultiplier = 1f;
        ResetTransientCharacterState();
        deck?.HandleLifeLost();
    }

    private void ApplyNewRoundResetPolicy()
    {
        currentDamage = 0;
        fallLives = 3;
        activeDefenseMultiplier = 1f;
        ResetTransientCharacterState();

        if (controller != null)
        {
            controller.Shield?.ResetShield();
            controller.Roll?.ResetRolls();
            controller.Dodge?.ResetDodges();
            controller.Dash?.ResetDash();
        }

        deck?.ResetDeckForNewRound();
    }

    private void ResetTransientCharacterState()
    {
        IsIntangible = false;

        GetComponent<AntiGravityEffect>()?.CancelEffect();

        if (controller == null)
            return;

        controller.Grab?.ReleaseGrabbedTarget();
        controller.ActiveInput?.ClearAllInputs();
        controller.ChangeState(controller.States.Idle);
        controller.ResetJumps();
        controller.Movement.ResetKnockback();
        controller.Movement.moveSpeedMultiplier = 1f;
        controller.Combat.attackSpeedMultiplier = 1f;
        controller.Shield?.Deactivate();
    }

    private void SetPhase(RespawnPhase phase)
    {
        Phase = phase;
    }

    private void SetCharacterVisible(bool visible)
    {
        if (sprite != null)
            sprite.enabled = visible;
    }

}
