using UnityEngine;

// The effect is parented to the afflicted character, so its lifetime and visuals stay together.
public class MirrorWorldLogic : MonoBehaviour
{
    private const int RingSegments = 32;
    private const float CastAnimationDuration = 0.35f;
    private const float EchoDuration = 0.3f;
    private static readonly Color PortalColor = new Color(0.45f, 0.2f, 1f, 1f);
    private static readonly Color MirrorColor = new Color(0.25f, 0.95f, 1f, 1f);

    [SerializeField, Min(0.1f)] private float effectDuration = 5f;

    private CharacterCoordinator caster;
    private CharacterCoordinator target;
    private CharacterHealth targetHealth;
    private SpriteRenderer targetSprite;
    private SpriteRenderer echo;
    private LineRenderer timerRing;
    private LineRenderer leftArrow;
    private LineRenderer rightArrow;
    private LineRenderer castRing;
    private LineRenderer castLink;
    private GameObject castVisual;
    private Vector3 castOrigin;
    private float activatedAt;
    private float expiresAt;
    private bool active;

    public void Initialize(CharacterCoordinator source, CharacterCoordinator afflicted)
    {
        caster = source;
        target = afflicted;
        targetHealth = afflicted != null ? afflicted.Health : null;
        targetSprite = afflicted != null ? afflicted.Sprite : null;
    }

    public void Activate()
    {
        if (target == null || targetHealth == null || targetHealth.Phase != RespawnPhase.Active)
        {
            CancelEffect();
            return;
        }

        active = true;
        activatedAt = Time.time;
        expiresAt = activatedAt + Mathf.Max(0.1f, effectDuration);
        target.SetHorizontalInputMirrored(true);

        EnsureIndicator();
        PlayCastAnimation();
        PlayEcho();
        UpdateVisuals();
    }

    private void Update()
    {
        if (!active)
            return;

        if (target == null || targetHealth == null || targetHealth.Phase != RespawnPhase.Active ||
            Time.time >= expiresAt)
        {
            if (target != null && targetHealth != null && targetHealth.Phase == RespawnPhase.Active)
                PlayShatter();
            CancelEffect();
            return;
        }

        UpdateVisuals();
    }

    public void CancelEffect()
    {
        bool wasActive = active;
        active = false;
        if (wasActive && target != null)
            target.SetHorizontalInputMirrored(false);
        if (castVisual != null)
            Destroy(castVisual);
        if (echo != null)
            Destroy(echo.gameObject);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (active)
            CancelEffect();
    }

    private void EnsureIndicator()
    {
        if (timerRing != null)
            return;

        Transform visual = new GameObject("Mirror World Indicator").transform;
        visual.SetParent(transform, false);
        timerRing = CreateLine("Remaining time", visual, 0.055f);
        leftArrow = CreateLine("Left arrow", visual, 0.065f);
        rightArrow = CreateLine("Right arrow", visual, 0.065f);
        leftArrow.positionCount = 5;
        rightArrow.positionCount = 5;
    }

    private void PlayCastAnimation()
    {
        if (castVisual != null)
            Destroy(castVisual);

        castOrigin = caster != null && caster.Sprite != null
            ? caster.Sprite.bounds.center
            : caster != null ? caster.transform.position : target.transform.position;
        castVisual = new GameObject("Mirror World Cast");
        castRing = CreateLine("Portal", castVisual.transform, 0.075f);
        castLink = CreateLine("Mirror beam", castVisual.transform, 0.08f);
        castRing.positionCount = RingSegments + 1;
        castLink.positionCount = 2;
    }

    private void PlayEcho()
    {
        if (echo != null)
            Destroy(echo.gameObject);
        if (targetSprite == null)
            return;

        GameObject ghost = new GameObject("Mirror Echo");
        ghost.transform.SetParent(targetSprite.transform, false);
        echo = ghost.AddComponent<SpriteRenderer>();
        echo.sortingLayerID = targetSprite.sortingLayerID;
        echo.sortingOrder = targetSprite.sortingOrder + 2;
    }

    private void UpdateVisuals()
    {
        Vector3 center = targetSprite != null ? targetSprite.bounds.center : target.transform.position;
        float radius = targetSprite != null ? Mathf.Max(0.65f, targetSprite.bounds.extents.y + 0.17f) : 0.8f;
        float remaining = Mathf.Clamp01((expiresAt - Time.time) / Mathf.Max(0.1f, effectDuration));
        float pulse = 1f - Mathf.Clamp01((Time.time - activatedAt) / CastAnimationDuration);

        DrawRing(timerRing, center, radius * (1f + 0.2f * pulse), remaining);
        SetColor(timerRing, Color.Lerp(PortalColor, MirrorColor, 0.5f));
        float arrowY = center.y + radius + 0.23f;
        DrawArrow(leftArrow, center.x + 0.32f, center.x - 0.32f, arrowY + 0.1f);
        DrawArrow(rightArrow, center.x - 0.32f, center.x + 0.32f, arrowY - 0.1f);
        SetColor(leftArrow, MirrorColor);
        SetColor(rightArrow, PortalColor);

        UpdateCastAnimation(center);
        UpdateEcho();
    }

    private void UpdateCastAnimation(Vector3 targetCenter)
    {
        if (castVisual == null)
            return;

        float progress = Mathf.Clamp01((Time.time - activatedAt) / CastAnimationDuration);
        if (progress >= 1f)
        {
            Destroy(castVisual);
            castVisual = null;
            return;
        }

        DrawRing(castRing, castOrigin, 0.25f + progress * 0.55f, 1f);
        castLink.SetPosition(0, castOrigin);
        castLink.SetPosition(1, targetCenter);
        SetColor(castRing, new Color(PortalColor.r, PortalColor.g, PortalColor.b, 1f - progress));
        SetColor(castLink, new Color(MirrorColor.r, MirrorColor.g, MirrorColor.b, 1f - progress));
    }

    private void UpdateEcho()
    {
        if (echo == null || targetSprite == null)
            return;

        float progress = (Time.time - activatedAt) / EchoDuration;
        if (progress >= 1f)
        {
            Destroy(echo.gameObject);
            echo = null;
            return;
        }

        echo.sprite = targetSprite.sprite;
        echo.flipX = !targetSprite.flipX;
        echo.color = new Color(MirrorColor.r, MirrorColor.g, MirrorColor.b, 0.65f * (1f - progress));
    }

    private void PlayShatter()
    {
        GameObject fragments = new GameObject("Mirror World Shatter", typeof(ParticleSystem));
        fragments.transform.position = targetSprite != null ? targetSprite.bounds.center : target.transform.position;
        ParticleSystem particles = fragments.GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = 0.35f;
        main.startSpeed = 2f;
        main.startSize = 0.09f;
        main.startColor = new ParticleSystem.MinMaxGradient(PortalColor, MirrorColor);
        main.maxParticles = 20;
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        ParticleSystemRenderer renderer = fragments.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = Resources.Load<Material>("HitImpactParticles");
        renderer.sortingLayerID = targetSprite != null ? targetSprite.sortingLayerID : 0;
        renderer.sortingOrder = targetSprite != null ? targetSprite.sortingOrder + 3 : 3;
        particles.Play();
        particles.Emit(16);
        Destroy(fragments, 0.7f);
    }

    private LineRenderer CreateLine(string lineName, Transform parent, float width)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(parent, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.sharedMaterial = Resources.Load<Material>("HitImpactParticles");
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 2;
        line.sortingLayerID = targetSprite != null ? targetSprite.sortingLayerID : 0;
        line.sortingOrder = targetSprite != null ? targetSprite.sortingOrder + 3 : 3;
        return line;
    }

    private static void DrawRing(LineRenderer line, Vector3 center, float radius, float fraction)
    {
        line.positionCount = RingSegments + 1;
        for (int i = 0; i <= RingSegments; i++)
        {
            float angle = -Mathf.PI * 0.5f + 2f * Mathf.PI * fraction * i / RingSegments;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private static void DrawArrow(LineRenderer line, float startX, float endX, float y)
    {
        float head = endX > startX ? -0.14f : 0.14f;
        line.SetPosition(0, new Vector3(startX, y, 0f));
        line.SetPosition(1, new Vector3(endX, y, 0f));
        line.SetPosition(2, new Vector3(endX + head, y + 0.11f, 0f));
        line.SetPosition(3, new Vector3(endX, y, 0f));
        line.SetPosition(4, new Vector3(endX + head, y - 0.11f, 0f));
    }

    private static void SetColor(LineRenderer line, Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }
}
