using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CombatFeedback : MonoBehaviour
{
    private const string ImpactParticleMaterialResourcePath = "HitImpactParticles";
    private const string AudioSettingsResourcePath = "CombatAudioSettings";
    private const string CameraSettingsResourcePath = "CombatCameraSettings";

    private static CombatFeedback instance;
    private static Material impactParticleMaterial;
    private static CombatCameraSettings cameraSettings;

    public static bool IsHitStopActive => instance != null && instance.hitStopRoutine != null;

    private CinemachineImpulseSource impulseSource;
    private CinemachineImpulseManager.ImpulseEvent activeCameraImpulse;
    private float activeCameraImpulseEndTime;
    private CombatAudioSettings audioSettings;
    private AudioSource hitAudioSource;
    private Coroutine hitStopRoutine;
    private Coroutine impactFlashRoutine;
    private readonly Dictionary<Gamepad, Coroutine> rumbleRoutines = new();
    private readonly List<CinemachineImpulseListener> impulseListeners = new();
    private CinemachineBrain cameraBrain;
    private CinemachineImpactZoom impactZoom;
    private Image impactFlash;
    private bool previousBrainIgnoreTimeScale;
    private bool previousImpulseIgnoreTimeScale;
    private float hitStopEndTime;
    private float timeScaleBeforeHitStop = 1f;

    // Called only by an attack hitbox after the receiver accepts the hit.
    // Keeping this separate from PlayImpact avoids adding melee audio to cards.
    public static void PlayHitSound(AudioClip overrideClip = null)
    {
        CombatFeedback feedback = GetOrCreate();
        AudioClip clip = overrideClip != null ? overrideClip : feedback.audioSettings?.hitClip;
        if (clip == null)
            return;

        if (feedback.hitAudioSource == null)
        {
            feedback.hitAudioSource = feedback.gameObject.AddComponent<AudioSource>();
            feedback.hitAudioSource.playOnAwake = false;
            feedback.hitAudioSource.loop = false;
            feedback.hitAudioSource.spatialBlend = 0f;
            GameSettings.RegisterSource(feedback.hitAudioSource, GameSound.SoundEffects);
        }

        float volume = feedback.audioSettings != null ? Mathf.Clamp01(feedback.audioSettings.hitVolume) : 1f;
        feedback.hitAudioSource.PlayOneShot(clip, volume);
    }

    public static void PlayHitRumble(CharacterCoordinator defender, HitReaction reaction)
    {
        if (reaction == HitReaction.StrongHit || reaction == HitReaction.Stunned)
            GetOrCreate().Rumble(defender, 0.55f, 0.8f, 0.18f);
        else
            GetOrCreate().Rumble(defender, 0.25f, 0.5f, 0.09f);
    }

    public static void PlayParryRumble(CharacterCoordinator defender)
    {
        GetOrCreate().Rumble(defender, 0.1f, 0.65f, 0.07f);
    }

    public static void PlayStockLossRumble(CharacterCoordinator defender)
    {
        GetOrCreate().Rumble(defender, 0.75f, 0.45f, 0.3f);
    }

    public static void StopAllRumble()
    {
        if (instance != null)
            instance.StopRumble();
    }

    public static void ApplyScreenShakeIntensity()
    {
        if (instance == null)
            return;

        foreach (CinemachineImpulseListener listener in instance.impulseListeners)
            if (listener != null)
                listener.Gain = GameSettings.ScreenShakeIntensity;

        if (GameSettings.ScreenShakeIntensity <= 0f)
            instance.CancelCameraImpulse();

        if (GameSettings.ScreenShakeIntensity <= 0f && instance.impactFlash != null)
        {
            if (instance.impactFlashRoutine != null)
                instance.StopCoroutine(instance.impactFlashRoutine);
            instance.impactFlashRoutine = null;
            instance.impactFlash.color = Color.clear;
        }
    }

    private void Rumble(CharacterCoordinator character, float low, float high, float duration)
    {
        if (character == null || character.Mode != PlayerMode.Player || GameSettings.RumbleIntensity <= 0f)
            return;

        PlayerInput playerInput = character.GetComponent<PlayerInput>();
        if (playerInput == null)
            return;

        foreach (InputDevice device in playerInput.devices)
        {
            if (device is not Gamepad gamepad)
                continue;

            if (rumbleRoutines.TryGetValue(gamepad, out Coroutine previous))
                StopCoroutine(previous);

            gamepad.SetMotorSpeeds(low * GameSettings.RumbleIntensity, high * GameSettings.RumbleIntensity);
            rumbleRoutines[gamepad] = StartCoroutine(StopRumbleAfter(gamepad, duration));
            break;
        }
    }

    private IEnumerator StopRumbleAfter(Gamepad gamepad, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (gamepad != null)
            gamepad.ResetHaptics();
        rumbleRoutines.Remove(gamepad);
    }

    private void StopRumble()
    {
        foreach (KeyValuePair<Gamepad, Coroutine> entry in rumbleRoutines)
        {
            StopCoroutine(entry.Value);
            if (entry.Key != null)
                entry.Key.ResetHaptics();
        }

        rumbleRoutines.Clear();
    }

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction)
    {
        PlayImpact(position, knockback, reaction, null, 0);
    }

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, Color impactColor)
    {
        PlayImpact(position, knockback, reaction, (Color?)impactColor, 0);
    }

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, int visualChainStep)
    {
        PlayImpact(position, knockback, reaction, null, visualChainStep);
    }

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, Color impactColor, int visualChainStep)
    {
        PlayImpact(position, knockback, reaction, (Color?)impactColor, visualChainStep);
    }

    private static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, Color? impactColor, int visualChainStep)
    {
        CombatFeedback feedback = GetOrCreate();
        int step = Mathf.Clamp(visualChainStep, 0, 2);
        feedback.SpawnImpactParticles(position, knockback, reaction, impactColor, step);
        feedback.ShakeCamera(position, knockback, reaction, step);
        feedback.PlayDecisiveImpact(position, knockback, reaction);
        feedback.RequestHitStop(GetHitStopDuration(reaction));
    }

    private static CombatFeedback GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject feedbackObject = new GameObject(nameof(CombatFeedback));
        instance = feedbackObject.AddComponent<CombatFeedback>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSettings = Resources.Load<CombatAudioSettings>(AudioSettingsResourcePath);
        cameraSettings = Resources.Load<CombatCameraSettings>(CameraSettingsResourcePath);
        ConfigureCameraImpulse();
    }

    private void ConfigureCameraImpulse()
    {
        impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        impulseSource.ImpulseDefinition.ImpulseChannel = 1;
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulseSource.ImpulseDefinition.ImpulseDuration = 0.14f;
        impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;

        previousImpulseIgnoreTimeScale = CinemachineImpulseManager.Instance.IgnoreTimeScale;
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        cameraBrain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (cameraBrain != null)
        {
            previousBrainIgnoreTimeScale = cameraBrain.IgnoreTimeScale;
            cameraBrain.IgnoreTimeScale = true;
        }

        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (CinemachineCamera camera in cameras)
        {
            CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
                listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();

            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = GameSettings.ScreenShakeIntensity;
            listener.Use2DDistance = true;
            listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.UseLargest;
            impulseListeners.Add(listener);

            if (impactZoom == null)
            {
                impactZoom = camera.GetComponent<CinemachineImpactZoom>();
                if (impactZoom == null)
                    impactZoom = camera.gameObject.AddComponent<CinemachineImpactZoom>();
            }
        }
    }

    private void ShakeCamera(Vector2 position, Vector2 knockback, HitReaction reaction, int visualChainStep)
    {
        if (impulseSource == null || GameSettings.ScreenShakeIntensity <= 0f || PauseMenu.isPaused)
            return;

        float strength = CalculateCameraShakeStrength(knockback.magnitude, reaction, visualChainStep);
        Vector2 direction = knockback.sqrMagnitude > 0.0001f
            ? -knockback.normalized
            : Random.insideUnitCircle.normalized;

        float duration = reaction == HitReaction.Hit
            ? cameraSettings != null ? cameraSettings.lightShakeDuration : 0.1f
            : cameraSettings != null ? cameraSettings.strongShakeDuration : 0.16f;
        CancelCameraImpulse();
        impulseSource.ImpulseDefinition.ImpulseDuration = Mathf.Max(0f, duration);
        activeCameraImpulse = impulseSource.ImpulseDefinition.CreateAndReturnEvent(position, direction * strength);
        activeCameraImpulseEndTime = CinemachineImpulseManager.Instance.CurrentTime + duration;
    }

    private void CancelCameraImpulse()
    {
        CinemachineImpulseManager manager = CinemachineImpulseManager.Instance;
        if (activeCameraImpulse != null && manager.CurrentTime < activeCameraImpulseEndTime)
            activeCameraImpulse.Cancel(manager.CurrentTime, true);
        activeCameraImpulse = null;
    }

    public static float CalculateCameraShakeStrength(float launchSpeed, HitReaction reaction, int visualChainStep)
    {
        if (cameraSettings == null)
            cameraSettings = Resources.Load<CombatCameraSettings>(CameraSettingsResourcePath);

        float lightStrength = cameraSettings != null ? cameraSettings.minimumShakeStrength : 0.045f;
        float heavyStrength = cameraSettings != null ? cameraSettings.maximumShakeStrength : 0.14f;
        float strengthCap = cameraSettings != null ? cameraSettings.shakeStrengthCap : 0.23f;
        float chainBonus = cameraSettings != null ? cameraSettings.chainStepBonus : 0.18f;
        float launchStrength = Mathf.Lerp(lightStrength, heavyStrength,
            Mathf.InverseLerp(3f, 18f, launchSpeed));
        float reactionMultiplier = reaction switch
        {
            HitReaction.StrongHit => 1.25f,
            HitReaction.Stunned => 1.45f,
            _ => 1f
        };
        return Mathf.Min(Mathf.Max(0f, strengthCap),
            launchStrength * reactionMultiplier * (1f + chainBonus * Mathf.Clamp(visualChainStep, 0, 2)));
    }

    private void PlayDecisiveImpact(Vector2 position, Vector2 knockback, HitReaction reaction)
    {
        if (reaction != HitReaction.StrongHit || knockback.magnitude < 15f ||
            impactZoom == null || GameSettings.ScreenShakeIntensity <= 0f || PauseMenu.isPaused)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 viewport = camera.WorldToViewportPoint(position);
        if (viewport.z <= 0f || viewport.x < 0.15f || viewport.x > 0.85f ||
            viewport.y < 0.15f || viewport.y > 0.85f)
            return;

        impactZoom.Play();
        if (impactFlashRoutine != null)
            StopCoroutine(impactFlashRoutine);
        impactFlashRoutine = StartCoroutine(FlashImpact());
    }

    private IEnumerator FlashImpact()
    {
        if (impactFlash == null)
        {
            GameObject canvasObject = new GameObject("Decisive Impact Flash", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            GameObject imageObject = new GameObject("Flash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            impactFlash = imageObject.GetComponent<Image>();
            impactFlash.raycastTarget = false;
        }

        const float duration = 0.10f;
        float start = Time.realtimeSinceStartup;
        do
        {
            float remaining = 1f - Mathf.Clamp01((Time.realtimeSinceStartup - start) / duration);
            impactFlash.color = new Color(1f, 0.94f, 0.82f, 0.08f * remaining);
            yield return null;
        } while (Time.realtimeSinceStartup - start < duration);

        impactFlash.color = Color.clear;
        impactFlashRoutine = null;
    }

    private void SpawnImpactParticles(Vector2 position, Vector2 knockback, HitReaction reaction, Color? impactColor, int visualChainStep)
    {
        GameObject particleObject = new GameObject("HitImpactParticles");
        particleObject.transform.position = position;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, (reaction == HitReaction.Hit ? 3f : 5f) + 0.75f * visualChainStep);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f + 0.01f * visualChainStep,
            (reaction == HitReaction.Hit ? 0.14f : 0.22f) + 0.025f * visualChainStep);
        main.startColor = GetImpactParticleColors(reaction, impactColor);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 24;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        Vector2 impactDirection = knockback.sqrMagnitude > 0.0001f ? knockback.normalized : Vector2.up;
        velocity.x = impactDirection.x * 0.8f;
        velocity.y = impactDirection.y * 0.8f;

        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = 20;

        Material material = GetImpactParticleMaterial();
        if (material != null)
            // The shared material stays white so the owner's particle color remains authoritative.
            particleRenderer.sharedMaterial = material;

        int count = reaction switch
        {
            HitReaction.StrongHit => 16,
            HitReaction.Stunned => 20,
            _ => 9
        };
        particles.Emit(Mathf.Min(count + 4 * visualChainStep, main.maxParticles));
        particles.Play();
    }

    private static Material GetImpactParticleMaterial()
    {
        if (impactParticleMaterial != null)
            return impactParticleMaterial;

        impactParticleMaterial = Resources.Load<Material>(ImpactParticleMaterialResourcePath);
        if (impactParticleMaterial != null)
            return impactParticleMaterial;

        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            Debug.LogError("[CombatFeedback] URP particle shader was not found.");
            return null;
        }

        impactParticleMaterial = new Material(particleShader)
        {
            name = "HitImpactParticles (Runtime)",
            hideFlags = HideFlags.HideAndDontSave,
            renderQueue = 3000
        };
        impactParticleMaterial.SetFloat("_Surface", 1f);
        impactParticleMaterial.SetFloat("_SrcBlend", 5f);
        impactParticleMaterial.SetFloat("_DstBlend", 10f);
        impactParticleMaterial.SetFloat("_ZWrite", 0f);
        impactParticleMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        return impactParticleMaterial;
    }

    private static ParticleSystem.MinMaxGradient GetImpactParticleColors(HitReaction reaction, Color? impactColor)
    {
        if (impactColor.HasValue)
        {
            Color ownerColor = impactColor.Value;
            ownerColor.a = 1f;
            float highlightAmount = reaction == HitReaction.Hit ? 0.2f : 0.35f;
            Color highlightColor = Color.Lerp(ownerColor, Color.white, highlightAmount);
            return new ParticleSystem.MinMaxGradient(ownerColor, highlightColor);
        }

        return reaction == HitReaction.Stunned
            ? new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.7f, 1f), Color.white)
            : new ParticleSystem.MinMaxGradient(new Color(1f, 0.25f, 0.08f), new Color(1f, 0.9f, 0.25f));
    }

    private void RequestHitStop(float duration)
    {
        if (duration <= 0f || PauseMenu.isPaused)
            return;

        hitStopEndTime = Mathf.Max(hitStopEndTime, Time.realtimeSinceStartup + duration);
        if (hitStopRoutine != null)
            return;

        timeScaleBeforeHitStop = Time.timeScale;
        if (timeScaleBeforeHitStop <= 0f)
            return;

        Time.timeScale = 0f;
        hitStopRoutine = StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        while (Time.realtimeSinceStartup < hitStopEndTime)
            yield return null;

        RestoreTimeScale();
        hitStopRoutine = null;
    }

    private void RestoreTimeScale()
    {
        if (!PauseMenu.isPaused && Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = Mathf.Max(0.0001f, timeScaleBeforeHitStop);
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        StopRumble();
        CancelCameraImpulse();
        RestoreTimeScale();
        CinemachineImpulseManager.Instance.IgnoreTimeScale = previousImpulseIgnoreTimeScale;
        if (cameraBrain != null)
            cameraBrain.IgnoreTimeScale = previousBrainIgnoreTimeScale;
        instance = null;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StopRumble();
    }

    private static float GetHitStopDuration(HitReaction reaction)
    {
        return reaction switch
        {
            HitReaction.StrongHit => 0.08f,
            HitReaction.Stunned => 0.11f,
            _ => 0.045f
        };
    }
}
