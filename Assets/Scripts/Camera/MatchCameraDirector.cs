using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class MatchCameraDirector : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera matchCamera;

    [Header("Framing")]
    [SerializeField, Min(0f)] private float targetRadius = 1.5f;
    [SerializeField, Min(0f)] private float zoomOutDamping = 0.35f;
    [SerializeField, Min(0f)] private float zoomInDamping = 1.2f;
    [Tooltip("How long the wide shot is held after the fighters move closer together.")]
    [SerializeField, Min(0f)] private float zoomInHoldTime = 0.35f;

    [Header("Stage Focus")]
    [Tooltip("Center of the main fighting area, in world coordinates.")]
    [SerializeField] private Vector2 stageCenter = Vector2.zero;
    [Tooltip("Fraction of the CameraBounds radius that keeps a fighter at full camera weight.")]
    [SerializeField] private Vector2 fullFocusFraction = new Vector2(0.5f, 0.8f);
    [Tooltip("Fraction of the CameraBounds radius where a launched fighter stops affecting the camera.")]
    [SerializeField] private Vector2 lostFocusFraction = new Vector2(1.15f, 1.7f);

    private const float RosterRefreshInterval = 0.5f;
    private const float ZoomDeadband = 0.1f;
    private readonly List<CharacterHealth> observedHealth = new List<CharacterHealth>();
    private CinemachineTargetGroup targetGroup;
    private CinemachineGroupFraming groupFraming;
    private Camera outputCamera;
    private Collider2D stageBounds;
    private Transform stageAnchor;
    private Vector2 authoredOrthoRange;
    private float authoredDamping;
    private float refreshTimer;
    private float zoomInAllowedAt;

    private void Awake()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        groupFraming = matchCamera != null ? matchCamera.GetComponent<CinemachineGroupFraming>() : null;
        authoredOrthoRange = groupFraming != null ? groupFraming.OrthoSizeRange : Vector2.zero;
        authoredDamping = groupFraming != null ? groupFraming.Damping : 0f;
        outputCamera = Camera.main;

        CinemachineConfiner2D confiner = matchCamera != null ? matchCamera.GetComponent<CinemachineConfiner2D>() : null;
        stageBounds = confiner != null ? confiner.BoundingShape2D : null;
        if (stageBounds != null)
        {
            GameObject anchor = new GameObject("Camera Stage Focus");
            anchor.hideFlags = HideFlags.HideInHierarchy;
            stageAnchor = anchor.transform;
            stageAnchor.position = stageCenter;
        }
    }

    private void OnEnable()
    {
        RefreshTargets(true);
    }

    private void OnDisable()
    {
        UnsubscribeFromHealth();
        if (groupFraming != null)
        {
            groupFraming.OrthoSizeRange = authoredOrthoRange;
            groupFraming.Damping = authoredDamping;
        }
    }

    private void OnDestroy()
    {
        if (stageAnchor != null)
        {
            if (Application.isPlaying)
                Destroy(stageAnchor.gameObject);
            else
                DestroyImmediate(stageAnchor.gameObject);
        }
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = RosterRefreshInterval;
            RefreshTargets();
        }

        UpdateTargetWeights();
    }

    private void LateUpdate()
    {
        UpdateZoomDamping();
    }

    public void RefreshTargets(bool forceRefresh = false)
    {
        if (targetGroup == null)
            targetGroup = GetComponent<CinemachineTargetGroup>();

        CharacterCoordinator[] players = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        if (!forceRefresh && TargetsMatch(players))
            return;

        UnsubscribeFromHealth();
        targetGroup.Targets.Clear();
        if (stageAnchor != null)
            targetGroup.AddMember(stageAnchor, 1f, 0f);

        foreach (CharacterCoordinator player in players)
        {
            if (player == null || player.Health == null)
                continue;

            targetGroup.AddMember(player.transform, GetCameraWeight(player.Health), targetRadius);
            player.Health.CameraTrackingChanged += OnCameraTrackingChanged;
            observedHealth.Add(player.Health);
        }

        UpdateTargetWeights();
    }

    private bool TargetsMatch(CharacterCoordinator[] players)
    {
        if (targetGroup.Targets.Count != players.Length + (stageAnchor != null ? 1 : 0))
            return false;

        if (stageAnchor != null && targetGroup.FindMember(stageAnchor) < 0)
            return false;

        foreach (CharacterCoordinator player in players)
            if (player == null || player.Health == null || targetGroup.FindMember(player.transform) < 0)
                return false;

        return true;
    }

    private void OnCameraTrackingChanged(CharacterHealth health)
    {
        UpdateTargetWeights();
    }

    private void UnsubscribeFromHealth()
    {
        foreach (CharacterHealth health in observedHealth)
            if (health != null)
                health.CameraTrackingChanged -= OnCameraTrackingChanged;

        observedHealth.Clear();
    }

    private void UpdateTargetWeights()
    {
        if (targetGroup == null)
            return;

        int activeCount = 0;
        foreach (CharacterHealth health in observedHealth)
            if (GetCameraWeight(health) > 0f)
                activeCount++;

        foreach (CharacterHealth health in observedHealth)
        {
            if (health == null)
                continue;

            int index = targetGroup.FindMember(health.transform);
            if (index < 0)
                continue;

            float weight = GetCameraWeight(health);
            if (weight > 0f && activeCount > 1 && stageBounds != null)
                weight *= GetStageFocusWeight(health.transform.position);
            targetGroup.Targets[index].Weight = weight;
        }
    }

    private float GetStageFocusWeight(Vector2 position)
    {
        return CalculateFocusWeight(stageBounds.bounds, stageCenter, position,
            fullFocusFraction, lostFocusFraction);
    }

    public static float CalculateFocusWeight(Bounds bounds, Vector2 center, Vector2 position,
        Vector2 fullFraction, Vector2 lostFraction)
    {
        Vector2 radius = new Vector2(
            Mathf.Max(Mathf.Abs(bounds.min.x - center.x), Mathf.Abs(bounds.max.x - center.x)),
            Mathf.Max(Mathf.Abs(bounds.min.y - center.y), Mathf.Abs(bounds.max.y - center.y)));
        Vector2 distance = new Vector2(
            Mathf.Abs(position.x - center.x) / Mathf.Max(0.01f, radius.x),
            Mathf.Abs(position.y - center.y) / Mathf.Max(0.01f, radius.y));

        float horizontal = FocusWeight(distance.x, fullFraction.x, lostFraction.x);
        float vertical = FocusWeight(distance.y, fullFraction.y, lostFraction.y);
        return Mathf.Min(horizontal, vertical);
    }

    private static float FocusWeight(float distance, float fullWeightAt, float noWeightAt)
    {
        float t = Mathf.InverseLerp(fullWeightAt, Mathf.Max(fullWeightAt + 0.01f, noWeightAt), distance);
        return 1f - t * t * (3f - 2f * t);
    }

    private void UpdateZoomDamping()
    {
        if (groupFraming == null || matchCamera == null)
            return;

        float totalWeight = 0f;
        float maxWeight = 0f;
        Vector2 average = Vector2.zero;
        foreach (CinemachineTargetGroup.Target target in targetGroup.Targets)
        {
            if (target.Object == null || target.Weight <= 0f)
                continue;

            totalWeight += target.Weight;
            maxWeight = Mathf.Max(maxWeight, target.Weight);
            average += (Vector2)target.Object.position * target.Weight;
        }

        if (totalWeight <= 0f)
            return;

        average /= totalWeight;
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        foreach (CinemachineTargetGroup.Target target in targetGroup.Targets)
        {
            if (target.Object == null || target.Weight <= 0f)
                continue;

            float relativeWeight = target.Weight / maxWeight;
            Vector2 position = Vector2.Lerp(average, target.Object.position, relativeWeight);
            Vector2 radius = Vector2.one * target.Radius * relativeWeight;
            min = Vector2.Min(min, position - radius);
            max = Vector2.Max(max, position + radius);
        }

        float aspect = outputCamera != null ? outputCamera.aspect : 16f / 9f;
        float horizontalSize = (max.x - min.x) / (2f * Mathf.Max(0.01f, aspect));
        float verticalSize = (max.y - min.y) * 0.5f;
        float frameSize = groupFraming.FramingMode switch
        {
            CinemachineGroupFraming.FramingModes.Horizontal => horizontalSize,
            CinemachineGroupFraming.FramingModes.Vertical => verticalSize,
            _ => Mathf.Max(horizontalSize, verticalSize)
        };
        float desiredSize = Mathf.Clamp(frameSize / Mathf.Max(0.01f, groupFraming.FramingSize),
            authoredOrthoRange.x, authoredOrthoRange.y);

        float currentSize = matchCamera.State.Lens.OrthographicSize;
        if (currentSize <= 0f)
            currentSize = matchCamera.Lens.OrthographicSize;

        if (desiredSize > currentSize + ZoomDeadband)
        {
            zoomInAllowedAt = Time.time + zoomInHoldTime;
            groupFraming.OrthoSizeRange = authoredOrthoRange;
            groupFraming.Damping = zoomOutDamping;
        }
        else if (desiredSize < currentSize - ZoomDeadband && Time.time < zoomInAllowedAt)
        {
            // Hold the current lens size while keeping position tracking responsive.
            groupFraming.OrthoSizeRange = new Vector2(
                Mathf.Clamp(currentSize, authoredOrthoRange.x, authoredOrthoRange.y), authoredOrthoRange.y);
            groupFraming.Damping = zoomOutDamping;
        }
        else
        {
            groupFraming.OrthoSizeRange = authoredOrthoRange;
            groupFraming.Damping = zoomInDamping;
        }
    }

    private static float GetCameraWeight(CharacterHealth health)
    {
        return health != null && health.isActiveAndEnabled && health.ShouldCameraTrack ? 1f : 0f;
    }
}
