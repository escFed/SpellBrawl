using UnityEngine;

// A reusable, pixel-art contact burst. The sprites are generated once so every
// character and scene can use the same effect without prefab wiring.
public sealed class HitImpactVisual : MonoBehaviour
{
    private const int TextureSize = 32;
    private const int PixelsPerUnit = 64;
    private const int FrameCount = 5;

    private static Sprite[][] coreFrames;
    private static Sprite[][] accentFrames;

    private SpriteRenderer core;
    private SpriteRenderer accent;
    private ParticleSystem particles;
    private float elapsed;
    private float frameDuration;
    private float totalDuration;
    private int currentFrame;
    private int variant;

    public bool IsPlaying { get; private set; }
    public float LastPlayedAt { get; private set; }

    public void Initialize(Material particleMaterial)
    {
        EnsureSprites();

        core = CreateLayer("Impact core", 22);
        accent = CreateLayer("Impact accent", 21);

        particles = gameObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.075f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 24;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.035f;

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = 20;
        if (particleMaterial != null)
            particleRenderer.sharedMaterial = particleMaterial;

        gameObject.SetActive(false);
    }

    public void Play(Vector2 position, Vector2 knockback, HitReaction reaction, Color? impactColor, int chainStep)
    {
        gameObject.SetActive(true);
        transform.position = position;

        Vector2 direction = knockback.sqrMagnitude > 0.0001f ? knockback.normalized : Vector2.right;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        variant = reaction == HitReaction.Hit ? 0 : 1;
        float launchScale = Mathf.Lerp(0.85f, 1.25f, Mathf.InverseLerp(4f, 24f, knockback.magnitude));
        float scale = launchScale * (variant == 0 ? 1f : 1.3f) * (1f + 0.06f * chainStep);
        core.transform.localScale = Vector3.one * scale;
        accent.transform.localScale = Vector3.one * scale;

        Color color = impactColor ?? (reaction == HitReaction.Stunned
            ? new Color(0.35f, 0.7f, 1f)
            : new Color(1f, 0.35f, 0.12f));
        color.a = 1f;
        core.color = Color.white;
        accent.color = Color.Lerp(color, Color.white, 0.12f);

        currentFrame = 0;
        core.sprite = coreFrames[variant][0];
        accent.sprite = accentFrames[variant][0];
        elapsed = 0f;
        frameDuration = variant == 0 ? 0.04f : 0.05f;
        totalDuration = FrameCount * frameDuration;
        LastPlayedAt = Time.realtimeSinceStartup;
        IsPlaying = true;

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.55f));
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, variant == 0 ? 3.2f : 5.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, variant == 0 ? 0.07f : 0.095f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = direction.x * (variant == 0 ? 1.1f : 1.7f);
        velocity.y = direction.y * (variant == 0 ? 1.1f : 1.7f);

        particles.Play();
        particles.Emit(Mathf.Min((variant == 0 ? 6 : 10) + chainStep * 2, main.maxParticles));
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        // Scaled time holds the contact frame during combat hitstop and pause.
        elapsed += Time.deltaTime;
        if (elapsed >= totalDuration)
        {
            IsPlaying = false;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gameObject.SetActive(false);
            return;
        }

        int frame = Mathf.Min(Mathf.FloorToInt(elapsed / frameDuration), FrameCount - 1);
        if (frame == currentFrame)
            return;

        currentFrame = frame;
        core.sprite = coreFrames[variant][frame];
        accent.sprite = accentFrames[variant][frame];
    }

    private SpriteRenderer CreateLayer(string layerName, int sortingOrder)
    {
        GameObject layer = new GameObject(layerName, typeof(SpriteRenderer));
        layer.transform.SetParent(transform, false);
        SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static void EnsureSprites()
    {
        if (coreFrames != null && coreFrames[0][0] != null)
            return;

        coreFrames = new Sprite[2][];
        accentFrames = new Sprite[2][];
        for (int style = 0; style < 2; style++)
        {
            coreFrames[style] = new Sprite[FrameCount];
            accentFrames[style] = new Sprite[FrameCount];
            for (int frame = 0; frame < FrameCount; frame++)
            {
                coreFrames[style][frame] = BuildSprite(style == 1, frame, false);
                accentFrames[style][frame] = BuildSprite(style == 1, frame, true);
            }
        }
    }

    private static Sprite BuildSprite(bool strong, int frame, bool isAccent)
    {
        Color32[] pixels = new Color32[TextureSize * TextureSize];
        int rayCount = strong ? 8 : 6;
        float inner = frame switch { 0 => 1f, 1 => 3f, 2 => 6f, 3 => 10f, _ => 13f };
        float outer = frame switch { 0 => 6f, 1 => 11f, 2 => 14f, 3 => 15f, _ => 16f };

        if (!isAccent && frame < 3)
            FillSquare(pixels, 16, 16, frame == 1 ? 2 : 1);

        for (int ray = 0; ray < rayCount; ray++)
        {
            float angle = ray * Mathf.PI * 2f / rayCount;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float reach = direction.x > 0.2f ? 1.15f : 0.86f;

            if (isAccent)
            {
                // Offset colored fragments leave the bright core readable.
                DrawRay(pixels, direction, (inner + 2f) * reach, outer * reach, 1);
            }
            else if (frame < 4)
            {
                DrawRay(pixels, direction, inner * reach, (outer - 3f) * reach, 1);
            }
        }

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = strong ? "Heavy hit burst" : "Hit burst",
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static void DrawRay(Color32[] pixels, Vector2 direction, float start, float end, int width)
    {
        for (float distance = start; distance <= end; distance += 0.7f)
        {
            int x = Mathf.RoundToInt(16 + direction.x * distance);
            int y = Mathf.RoundToInt(16 + direction.y * distance);
            FillSquare(pixels, x, y, width);
        }
    }

    private static void FillSquare(Color32[] pixels, int centerX, int centerY, int radius)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            if (y < 0 || y >= TextureSize)
                continue;
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x >= 0 && x < TextureSize)
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, 255);
            }
        }
    }
}
