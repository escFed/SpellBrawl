using Unity.Cinemachine;
using UnityEngine;

// A short lens-only zoom leaves group framing and stage confinement in charge of position.
public sealed class CinemachineImpactZoom : CinemachineExtension
{
    private const float Duration = 0.18f;
    private const float MaximumZoom = 0.06f;
    private float startedAt = float.NegativeInfinity;

    public void Play()
    {
        startedAt = Time.realtimeSinceStartup;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize || !Application.isPlaying ||
            !state.Lens.Orthographic || GameSettings.ScreenShakeIntensity <= 0f)
            return;

        float elapsed = Time.realtimeSinceStartup - startedAt;
        if (elapsed < 0f || elapsed >= Duration)
            return;

        float zoom = MaximumZoom * Mathf.Sin(Mathf.PI * elapsed / Duration);
        LensSettings lens = state.Lens;
        lens.OrthographicSize *= 1f - zoom;
        state.Lens = lens;
    }
}
