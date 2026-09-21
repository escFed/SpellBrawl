using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    private const string ImpactParticleMaterialResourcePath = "HitImpactParticles";
    private const string AudioSettingsResourcePath = "CombatAudioSettings";

    private static CombatFeedback instance;
    private static Material impactParticleMaterial;

    public static bool IsHitStopActive => instance != null && instance.hitStopRoutine != null;

    private CinemachineImpulseSource impulseSource;
    private CombatAudioSettings audioSettings;
    private AudioSource hitAudioSource;
    private Coroutine hitStopRoutine;
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

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction)
    {
        PlayImpact(position, knockback, reaction, null);
    }

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, Color impactColor)
    {
        PlayImpact(position, knockback, reaction, (Color?)impactColor);
    }

    private static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction, Color? impactColor)
    {
        CombatFeedback feedback = GetOrCreate();
        feedback.SpawnImpactParticles(position, knockback, reaction, impactColor);
        feedback.ShakeCamera(position, knockback, reaction);
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
        ConfigureCameraImpulse();
    }

    private void ConfigureCameraImpulse()
    {
        impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        impulseSource.ImpulseDefinition.ImpulseChannel = 1;
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulseSource.ImpulseDefinition.ImpulseDuration = 0.14f;
        impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;

        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (CinemachineCamera camera in cameras)
        {
            CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
                listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();

            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = 1f;
            listener.Use2DDistance = true;
            listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.UseLargest;
        }
    }

    private void ShakeCamera(Vector2 position, Vector2 knockback, HitReaction reaction)
    {
        if (impulseSource == null)
            return;

        float strength = reaction switch
        {
            HitReaction.StrongHit => 0.18f,
            HitReaction.Stunned => 0.24f,
            _ => 0.08f
        };
        Vector2 direction = knockback.sqrMagnitude > 0.0001f
            ? -knockback.normalized
            : Random.insideUnitCircle.normalized;

        impulseSource.ImpulseDefinition.ImpulseDuration = reaction == HitReaction.Hit ? 0.1f : 0.16f;
        impulseSource.GenerateImpulseAtPositionWithVelocity(position, direction * strength);
    }

    private void SpawnImpactParticles(Vector2 position, Vector2 knockback, HitReaction reaction, Color? impactColor)
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
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, reaction == HitReaction.Hit ? 3f : 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, reaction == HitReaction.Hit ? 0.14f : 0.22f);
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
        particles.Emit(count);
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

        RestoreTimeScale();
        instance = null;
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
