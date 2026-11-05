using UnityEngine;

// The character owns Haste so its lifetime does not depend on the played card instance.
public sealed class HasteEffect : MonoBehaviour
{
    private const int RingSegments = 40;
    private const float ActivationPulseDuration = 0.3f;
    private static readonly Color ReadyColor = new Color(0.22f, 0.98f, 0.9f, 0.95f);
    private static readonly Color SpentColor = new Color(0.25f, 0.65f, 0.95f, 0.8f);

    private CharacterCoordinator character;
    private CharacterMovement movement;
    private CharacterHealth health;
    private SpriteRenderer characterSprite;
    private LineRenderer indicator;
    private ParticleSystem speedStreaks;
    private float expiresAt;
    private float activatedAt;
    private float duration;
    private float recoveryMultiplier = 1f;
    private float ringRadius;
    private bool active;
    private bool attackReady;

    public bool IsActive => active && health != null && health.Phase == RespawnPhase.Active && Time.time < expiresAt;

    public void Apply(float effectDuration, float moveMultiplier, float nextAttackRecoveryMultiplier)
    {
        if (character == null)
        {
            character = GetComponent<CharacterCoordinator>();
            movement = GetComponent<CharacterMovement>();
            health = GetComponent<CharacterHealth>();
            characterSprite = character != null ? character.Sprite : null;
        }

        if (movement == null || health == null || health.Phase != RespawnPhase.Active || effectDuration <= 0f)
            return;

        duration = effectDuration;
        expiresAt = Time.time + duration;
        activatedAt = Time.time;
        recoveryMultiplier = Mathf.Clamp(nextAttackRecoveryMultiplier, 0.1f, 1f);
        attackReady = true;
        active = true;
        movement.SetHasteMoveSpeedMultiplier(Mathf.Max(1f, moveMultiplier));
        EnsureIndicator();
        speedStreaks.Clear();
        speedStreaks.Play();
        speedStreaks.Emit(16);
        UpdateIndicator();
    }

    public float ConsumeAttackRecoveryMultiplier()
    {
        if (!IsActive || !attackReady)
            return 1f;

        attackReady = false;
        UpdateIndicator();
        return recoveryMultiplier;
    }

    private void Update()
    {
        if (!active)
            return;

        if (!IsActive)
        {
            CancelEffect();
            return;
        }

        UpdateIndicator();
    }

    public void CancelEffect()
    {
        active = false;
        attackReady = false;
        if (movement != null)
            movement.SetHasteMoveSpeedMultiplier(1f);
        if (speedStreaks != null)
            speedStreaks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (indicator != null)
            indicator.gameObject.SetActive(false);
    }

    private void OnDisable() => CancelEffect();

    private void EnsureIndicator()
    {
        if (indicator != null)
        {
            indicator.gameObject.SetActive(true);
            return;
        }

        GameObject ring = new GameObject("Haste Timer");
        ring.transform.SetParent(transform, false);
        indicator = ring.AddComponent<LineRenderer>();
        indicator.useWorldSpace = false;
        indicator.loop = false;
        indicator.startWidth = 0.055f;
        indicator.endWidth = 0.055f;
        indicator.numCapVertices = 3;
        indicator.sharedMaterial = Resources.Load<Material>("HitImpactParticles");
        if (characterSprite != null)
        {
            indicator.sortingLayerID = characterSprite.sortingLayerID;
            indicator.sortingOrder = characterSprite.sortingOrder + 1;
            ringRadius = Mathf.Max(0.7f, characterSprite.bounds.extents.y + 0.15f) /
                Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
        }
        else
        {
            ringRadius = 0.8f;
        }

        CreateSpeedStreaks(ring.transform);
    }

    private void CreateSpeedStreaks(Transform parent)
    {
        GameObject streakObject = new GameObject("Speed Streaks", typeof(ParticleSystem));
        streakObject.transform.SetParent(parent, false);
        speedStreaks = streakObject.GetComponent<ParticleSystem>();
        speedStreaks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = speedStreaks.main;
        main.playOnAwake = false;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.07f);
        main.startColor = new ParticleSystem.MinMaxGradient(ReadyColor, SpentColor);
        main.maxParticles = 64;

        ParticleSystem.ShapeModule shape = speedStreaks.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(ringRadius * 1.3f, ringRadius * 1.4f, 0.01f);

        ParticleSystem.VelocityOverLifetimeModule velocity = speedStreaks.velocityOverLifetime;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(1.5f, 2.3f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.enabled = true;

        ParticleSystem.EmissionModule emission = speedStreaks.emission;
        emission.rateOverTime = 42f;

        ParticleSystem.ColorOverLifetimeModule fade = speedStreaks.colorOverLifetime;
        fade.enabled = true;
        Gradient alpha = new Gradient();
        alpha.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.15f), new GradientAlphaKey(0f, 1f) });
        fade.color = alpha;

        ParticleSystemRenderer renderer = streakObject.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = indicator.sharedMaterial;
        renderer.sortingLayerID = indicator.sortingLayerID;
        renderer.sortingOrder = indicator.sortingOrder;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.velocityScale = 0.2f;
    }

    private void UpdateIndicator()
    {
        if (indicator == null)
            return;

        if (characterSprite != null)
            indicator.transform.localPosition = transform.InverseTransformPoint(characterSprite.bounds.center);

        Color color = attackReady ? ReadyColor : SpentColor;
        float pulse = 1f - Mathf.Clamp01((Time.time - activatedAt) / ActivationPulseDuration);
        float radius = ringRadius * (1f + 0.22f * pulse);
        indicator.startWidth = indicator.endWidth = 0.055f + 0.045f * pulse;
        indicator.startColor = color;
        indicator.endColor = color;

        float fraction = Mathf.Clamp01((expiresAt - Time.time) / duration);
        int segments = Mathf.Max(1, Mathf.CeilToInt(RingSegments * fraction));
        indicator.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * (0.5f + 2f * fraction * i / segments);
            indicator.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }
}
