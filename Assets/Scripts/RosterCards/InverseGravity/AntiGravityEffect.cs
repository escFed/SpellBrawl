using UnityEngine;

public class AntiGravityEffect : MonoBehaviour
{
    private const string ParticleMaterialPath = "HitImpactParticles";

    private CharacterMovement movement;
    private CharacterHealth health;
    private ParticleSystem aura;
    private float expiresAt;
    private bool active;

    public void Apply(float duration, float gravityMultiplier, float pulseSpeed)
    {
        if (movement == null)
            movement = GetComponent<CharacterMovement>();
        if (health == null)
            health = GetComponent<CharacterHealth>();

        if (movement == null || health == null || health.Phase != RespawnPhase.Active)
        {
            CancelEffect();
            return;
        }

        bool firstApplication = !active;
        active = true;
        expiresAt = Time.time + Mathf.Max(0f, duration);
        movement.SetLevitationGravityMultiplier(gravityMultiplier);

        if (firstApplication)
            movement.ApplyLevitationPulse(pulseSpeed);

        EnsureAura();
        aura?.Emit(16);
    }

    private void Update()
    {
        if (active && (health == null || health.Phase != RespawnPhase.Active || Time.time >= expiresAt))
            CancelEffect();
    }

    public void CancelEffect()
    {
        if (active && movement != null)
            movement.SetLevitationGravityMultiplier(1f);

        active = false;
        if (aura != null)
        {
            Destroy(aura.gameObject);
            aura = null;
        }

        Destroy(this);
    }

    private void OnDisable()
    {
        if (active && movement != null)
            movement.SetLevitationGravityMultiplier(1f);

        active = false;
        if (aura != null)
            Destroy(aura.gameObject);
    }

    private void EnsureAura()
    {
        if (aura != null)
            return;

        GameObject visual = new GameObject("Levitation Aura");
        visual.transform.SetParent(transform, false);
        aura = visual.AddComponent<ParticleSystem>();
        aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = aura.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.6f;
        main.startSpeed = 0.4f;
        main.startSize = 0.1f;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.4f, 0.85f, 1f, 0.9f), new Color(0.75f, 0.45f, 1f, 0.9f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 48;

        ParticleSystem.ShapeModule shape = aura.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.65f;

        ParticleSystem.EmissionModule emission = aura.emission;
        emission.rateOverTime = 25f;

        ParticleSystem.VelocityOverLifetimeModule velocity = aura.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 1.2f;

        ParticleSystemRenderer renderer = visual.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;
        renderer.sharedMaterial = Resources.Load<Material>(ParticleMaterialPath);
        aura.Play();
    }
}
