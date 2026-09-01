using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    private static CombatFeedback instance;

    public static bool IsHitStopActive => instance != null && instance.hitStopRoutine != null;

    private CinemachineImpulseSource impulseSource;
    private Coroutine hitStopRoutine;
    private float hitStopEndTime;
    private float timeScaleBeforeHitStop = 1f;

    public static void PlayImpact(Vector2 position, Vector2 knockback, HitReaction reaction)
    {
        CombatFeedback feedback = GetOrCreate();
        feedback.SpawnImpactParticles(position, knockback, reaction);
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

    private void SpawnImpactParticles(Vector2 position, Vector2 knockback, HitReaction reaction)
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
        main.startColor = reaction == HitReaction.Stunned
            ? new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.7f, 1f), Color.white)
            : new ParticleSystem.MinMaxGradient(new Color(1f, 0.25f, 0.08f), new Color(1f, 0.9f, 0.25f));
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

        int count = reaction switch
        {
            HitReaction.StrongHit => 16,
            HitReaction.Stunned => 20,
            _ => 9
        };
        particles.Emit(count);
        particles.Play();
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
