using UnityEngine;

// Presentation only. The card applies healing immediately; this effect can finish independently.
public sealed class HealEffect : MonoBehaviour
{
    private const float RingDuration = 0.55f;
    private const float TotalDuration = 0.85f;
    private const int RingSegments = 32;

    private SpriteRenderer characterSprite;
    private Transform visualRoot;
    private LineRenderer ring;
    private ParticleSystem motes;
    private float radius;
    private float elapsed;

    public void Play(SpriteRenderer sprite)
    {
        characterSprite = sprite;
        if (visualRoot == null)
            CreateVisual();

        elapsed = 0f;
        visualRoot.gameObject.SetActive(true);
        PositionVisual();
        UpdateRing();
        motes.Clear();
        motes.Play();
        motes.Emit(24);
    }

    private void Update()
    {
        if (visualRoot == null)
        {
            Destroy(this);
            return;
        }

        elapsed += Time.deltaTime;
        PositionVisual();
        UpdateRing();
        if (elapsed >= TotalDuration)
        {
            Destroy(visualRoot.gameObject);
            visualRoot = null;
            Destroy(this);
        }
    }

    private void CreateVisual()
    {
        GameObject root = new GameObject("Heal Effect");
        root.transform.SetParent(transform, false);
        visualRoot = root.transform;
        radius = characterSprite != null
            ? Mathf.Max(0.55f, characterSprite.bounds.extents.y + 0.12f)
            : 0.65f;
        radius /= Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));

        Material material = Resources.Load<Material>("HitImpactParticles");
        ring = root.AddComponent<LineRenderer>();
        ring.sharedMaterial = material;
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = RingSegments;
        ring.startWidth = 0.055f;
        ring.endWidth = 0.055f;
        ring.sortingLayerID = characterSprite != null ? characterSprite.sortingLayerID : 0;
        ring.sortingOrder = characterSprite != null ? characterSprite.sortingOrder + 1 : 1;

        GameObject particles = new GameObject("Rising motes", typeof(ParticleSystem));
        particles.transform.SetParent(visualRoot, false);
        motes = particles.GetComponent<ParticleSystem>();
        motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = motes.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.09f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.3f, 1f, 0.4f, 1f), new Color(0.78f, 1f, 0.65f, 1f));
        main.maxParticles = 32;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.ShapeModule shape = motes.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius * 0.7f;
        ParticleSystem.EmissionModule emission = motes.emission;
        emission.rateOverTime = 0f;
        ParticleSystem.VelocityOverLifetimeModule velocity = motes.velocityOverLifetime;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.enabled = true;
        ParticleSystem.ColorOverLifetimeModule fade = motes.colorOverLifetime;
        fade.enabled = true;
        Gradient alpha = new Gradient();
        alpha.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        fade.color = alpha;

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingLayerID = ring.sortingLayerID;
        renderer.sortingOrder = ring.sortingOrder;
    }

    private void PositionVisual()
    {
        if (characterSprite != null)
            visualRoot.position = characterSprite.bounds.center;
    }

    private void UpdateRing()
    {
        float progress = Mathf.Clamp01(elapsed / RingDuration);
        float size = radius * Mathf.Lerp(0.45f, 1.2f, progress);
        Color color = new Color(0.35f, 1f, 0.48f, 0.9f * (1f - progress));
        ring.startColor = color;
        ring.endColor = color;
        ring.enabled = progress < 1f;
        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * size, Mathf.Sin(angle) * size, 0f));
        }
    }

    private void OnDisable()
    {
        if (visualRoot != null)
        {
            Destroy(visualRoot.gameObject);
            visualRoot = null;
        }
    }
}
