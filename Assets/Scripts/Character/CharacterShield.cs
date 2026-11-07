using UnityEngine;

public class CharacterShield : MonoBehaviour
{
    private const float ActiveDuration = 5f;
    private const int MaximumResistance = 50;
    private const float CooldownDuration = 5f;

    private static readonly Color ShieldColor = new Color(0.2f, 0.55f, 1f, 1f);

    public bool IsActive { get; private set; }
    public bool CanActivate => !IsActive && Time.time >= cooldownEndsAt;
    public int RemainingResistance { get; private set; }
    public float RemainingCooldown => Mathf.Max(0f, cooldownEndsAt - Time.time);
    public float CooldownLength => CooldownDuration;

    private SpriteRenderer characterSprite;
    private AmiroGuardVisuals guardVisuals;
    private Color colorBeforeShield;
    private float activeEndsAt;
    private float cooldownEndsAt;

    public void Initialize(SpriteRenderer sprite)
    {
        characterSprite = sprite;
        guardVisuals = GetComponent<AmiroGuardVisuals>();

        if (characterSprite != null)
            colorBeforeShield = characterSprite.color;
    }

    private void Update()
    {
        if (IsActive && Time.time >= activeEndsAt)
            Deactivate();
    }

    public bool TryActivate()
    {
        if (!CanActivate)
            return false;

        IsActive = true;
        RemainingResistance = MaximumResistance;
        activeEndsAt = Time.time + ActiveDuration;

        if (guardVisuals != null)
            guardVisuals.OnShieldStarted();
        else if (characterSprite != null)
        {
            colorBeforeShield = characterSprite.color;
            characterSprite.color = ShieldColor;
        }

        return true;
    }

    public int AbsorbDamage(int incomingDamage, Vector2 hitPoint)
    {
        incomingDamage = Mathf.Max(0, incomingDamage);

        if (!IsActive || incomingDamage == 0)
            return incomingDamage;

        int absorbedDamage = Mathf.Min(RemainingResistance, incomingDamage);
        RemainingResistance -= absorbedDamage;

        if (absorbedDamage > 0)
            CombatFeedback.PlayBlockSound();

        guardVisuals?.OnShieldImpact((float)RemainingResistance / MaximumResistance, hitPoint);

        if (RemainingResistance == 0)
            Break();

        return incomingDamage - absorbedDamage;
    }

    public void Break()
    {
        if (!IsActive) return;
        guardVisuals?.OnShieldBroken();
        DeactivateInternal(true);
    }

    public void Deactivate()
    {
        DeactivateInternal(false);
    }

    private void DeactivateInternal(bool broken)
    {
        if (!IsActive)
            return;

        IsActive = false;
        RemainingResistance = 0;
        cooldownEndsAt = Time.time + CooldownDuration;
        if (!broken) guardVisuals?.OnShieldStopped();
        RestoreSpriteColor();
    }

    public void ResetShield()
    {
        IsActive = false;
        RemainingResistance = 0;
        activeEndsAt = 0f;
        cooldownEndsAt = 0f;
        guardVisuals?.OnShieldStopped();
        RestoreSpriteColor();
    }

    private void OnDisable()
    {
        if (!IsActive)
            return;

        IsActive = false;
        RemainingResistance = 0;
        guardVisuals?.OnShieldStopped();
        RestoreSpriteColor();
    }

    private void RestoreSpriteColor()
    {
        if (guardVisuals == null && characterSprite != null)
            characterSprite.color = colorBeforeShield;
    }
}
