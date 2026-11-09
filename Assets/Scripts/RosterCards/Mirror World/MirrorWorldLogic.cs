using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The effect is parented to the afflicted character, so its lifetime and visuals stay together.
public class MirrorWorldLogic : MonoBehaviour
{
    private const int RingSegments = 32;
    private const float CastAnimationDuration = 0.35f;
    private const float EchoDuration = 0.3f;
    private static readonly Color PortalColor = new Color(0.45f, 0.2f, 1f, 1f);
    private static readonly Color MirrorColor = new Color(0.25f, 0.95f, 1f, 1f);

    [SerializeField, Min(0.1f)] private float effectDuration = 5f;
    [SerializeField, Min(0.1f)] private float soundSaturationDuration = 3f;
    [SerializeField, Range(0f, 1f)] private float soundDistortionLevel = 0.75f;
    [SerializeField, Min(1f)] private float battleMusicBpm = 120f;
    [SerializeField] private float musicBeatOffsetSeconds;
    [SerializeField] private Color screenTint = new Color(1f, 0f, 0f, 0.2f);

    private readonly List<AudioFilterState> saturatedAudioFilters = new List<AudioFilterState>();
    private Coroutine soundSaturationRoutine;
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
   [SerializeField] private Camera affectedCamera;
    private Color originalCameraColor;
    private bool cameraColorChanged;
    private GameObject screenTintCanvas;

    [SerializeField] private AudioClip cardClip;
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
        affectedCamera = Camera.main;
        if (affectedCamera != null)
        {
            originalCameraColor = affectedCamera.backgroundColor;
            cameraColorChanged = true;
            ChangeCameraColor(Color.red);
        }
        ShowScreenTint();
        StartSoundSaturation();

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
        if (cameraColorChanged && affectedCamera != null)
            affectedCamera.backgroundColor = originalCameraColor;
        cameraColorChanged = false;
        affectedCamera = null;
        if (screenTintCanvas != null)
            Destroy(screenTintCanvas);
        screenTintCanvas = null;
        StopSoundSaturation();
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

    private void StartSoundSaturation()
    {
        StopSoundSaturation();
        soundSaturationRoutine = StartCoroutine(SaturateSoundOnNextBeat());
    }

    private IEnumerator SaturateSoundOnNextBeat()
    {
        AudioSource music = CountdownManager.Instance != null
            ? CountdownManager.Instance.battleMusic
            : null;
        if (music != null && music.isPlaying && music.clip != null && battleMusicBpm > 0f)
        {
            double beatDuration = 60.0 / battleMusicBpm;
            double musicTime = music.timeSamples / (double)music.clip.frequency;
            double beatPosition = (musicTime - musicBeatOffsetSeconds) % beatDuration;
            if (beatPosition < 0d)
                beatPosition += beatDuration;

            double delayUntilBeat = beatPosition < 0.001d
                ? 0d
                : beatDuration - beatPosition;
            if (delayUntilBeat > 0d)
                yield return new WaitForSecondsRealtime((float)delayUntilBeat);
        }

        ApplySoundSaturation();
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, soundSaturationDuration));
        RestoreSoundSaturation();
        soundSaturationRoutine = null;
    }

    private void ApplySoundSaturation()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;

            AudioDistortionFilter filter = source.GetComponent<AudioDistortionFilter>();
            bool addedFilter = filter == null;
            if (addedFilter)
                filter = source.gameObject.AddComponent<AudioDistortionFilter>();

            saturatedAudioFilters.Add(new AudioFilterState(
                filter, addedFilter, filter.enabled, filter.distortionLevel));
            filter.enabled = true;
            filter.distortionLevel = soundDistortionLevel;
        }
    }

    private void StopSoundSaturation()
    {
        if (soundSaturationRoutine != null)
        {
            StopCoroutine(soundSaturationRoutine);
            soundSaturationRoutine = null;
        }

        RestoreSoundSaturation();
    }

    private void RestoreSoundSaturation()
    {
        foreach (AudioFilterState state in saturatedAudioFilters)
        {
            if (state.Filter == null)
                continue;

            if (state.AddedFilter)
            {
                Destroy(state.Filter);
                continue;
            }

            state.Filter.distortionLevel = state.OriginalDistortionLevel;
            state.Filter.enabled = state.WasEnabled;
        }

        saturatedAudioFilters.Clear();
    }

    private sealed class AudioFilterState
    {
        public AudioDistortionFilter Filter { get; }
        public bool AddedFilter { get; }
        public bool WasEnabled { get; }
        public float OriginalDistortionLevel { get; }

        public AudioFilterState(
            AudioDistortionFilter filter,
            bool addedFilter,
            bool wasEnabled,
            float originalDistortionLevel)
        {
            Filter = filter;
            AddedFilter = addedFilter;
            WasEnabled = wasEnabled;
            OriginalDistortionLevel = originalDistortionLevel;
        }
    }


    private void ShowScreenTint()
    {
        if (screenTintCanvas != null)
            Destroy(screenTintCanvas);

        screenTintCanvas = new GameObject("Mirror World Screen Tint", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = screenTintCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10000;

        GameObject tintObject = new GameObject("Red Tint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tintObject.transform.SetParent(screenTintCanvas.transform, false);
        RectTransform tintRect = tintObject.GetComponent<RectTransform>();
        tintRect.anchorMin = Vector2.zero;
        tintRect.anchorMax = Vector2.one;
        tintRect.offsetMin = Vector2.zero;
        tintRect.offsetMax = Vector2.zero;

        Image tintImage = tintObject.GetComponent<Image>();
        tintImage.color = screenTint;
        tintImage.raycastTarget = false;
    }

    private static void ChangeCameraColor(Color color)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            mainCamera.backgroundColor = color;
    }
}
