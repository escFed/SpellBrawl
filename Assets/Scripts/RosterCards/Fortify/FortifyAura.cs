using UnityEngine;

// Visual feedback only. CharacterHealth owns the immunity timer.
public sealed class FortifyAura : MonoBehaviour
{
    private const string ParticleMaterialPath = "HitImpactParticles";

    private CharacterHealth health;
    private SpriteRenderer characterSprite;
    private Transform visualRoot;
    private ParticleSystem ring;
    private ParticleSystem activationSparks;
    private float elapsed;

    public void Play(CharacterHealth protectedHealth, SpriteRenderer sprite)
    {
        health = protectedHealth;
        characterSprite = sprite;
        if (health == null || !health.IsDamageImmune)
            return;

        if (visualRoot == null)
            CreateVisual();

        elapsed = 0f;
        visualRoot.gameObject.SetActive(true);
        PositionAura();
        ring.Clear();
        ring.Play();
        ring.Emit(24);
        activationSparks.Clear();
        activationSparks.Play();
        activationSparks.Emit(18);
    }

    private void Update()
    {
        if (health == null || !health.IsDamageImmune)
        {
            Stop();
            return;
        }

        if (visualRoot == null)
            return;

        elapsed += Time.deltaTime;
        PositionAura();
        visualRoot.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(elapsed * 12f));
    }

    private void PositionAura()
    {
        if (characterSprite != null)
            visualRoot.position = characterSprite.bounds.center;
    }

    private void CreateVisual()
    {
        GameObject root = new GameObject("Fortify Aura");
        root.transform.SetParent(transform, false);
        visualRoot = root.transform;

        float radius = characterSprite != null
            ? Mathf.Max(0.65f, characterSprite.bounds.extents.y + 0.18f)
            : 0.75f;
        radius /= Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
        Material material = Resources.Load<Material>(ParticleMaterialPath);

        ring = CreateParticles("Amber ring", material);
        ParticleSystem.MainModule ringMain = ring.main;
        ringMain.loop = true;
        ringMain.startLifetime = 0.5f;
        ringMain.startSpeed = 0f;
        ringMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.08f);
        ringMain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.58f, 0.12f, 0.85f), new Color(1f, 0.9f, 0.48f, 0.95f));
        ringMain.maxParticles = 64;

        ParticleSystem.ShapeModule ringShape = ring.shape;
        ringShape.enabled = true;
        ringShape.shapeType = ParticleSystemShapeType.Circle;
        ringShape.radius = radius;
        ringShape.radiusThickness = 0.06f;
        ParticleSystem.EmissionModule ringEmission = ring.emission;
        ringEmission.rateOverTime = 80f;

        activationSparks = CreateParticles("Activation sparks", material);
        ParticleSystem.MainModule sparksMain = activationSparks.main;
        sparksMain.loop = false;
        sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        sparksMain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.55f, 0.1f, 0.9f), new Color(1f, 0.95f, 0.6f, 1f));
        sparksMain.maxParticles = 24;

        ParticleSystem.ShapeModule sparksShape = activationSparks.shape;
        sparksShape.enabled = true;
        sparksShape.shapeType = ParticleSystemShapeType.Circle;
        sparksShape.radius = radius * 0.7f;
        ParticleSystem.EmissionModule sparksEmission = activationSparks.emission;
        sparksEmission.rateOverTime = 0f;
    }

    private ParticleSystem CreateParticles(string name, Material material)
    {
        GameObject effect = new GameObject(name, typeof(ParticleSystem));
        effect.transform.SetParent(visualRoot, false);
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerID = characterSprite != null ? characterSprite.sortingLayerID : 0;
        renderer.sortingOrder = characterSprite != null ? characterSprite.sortingOrder + 1 : 1;
        renderer.sharedMaterial = material;
        return particles;
    }

    private void Stop()
    {
        if (visualRoot != null)
        {
            visualRoot.gameObject.SetActive(false);
            Destroy(visualRoot.gameObject);
            visualRoot = null;
        }
        Destroy(this);
    }

    private void OnDisable()
    {
        if (visualRoot != null)
            Destroy(visualRoot.gameObject);
    }
}
