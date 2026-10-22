using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(CinemachineTargetGroup))]
public class CountdownCameraFocus : MonoBehaviour
{
    [SerializeField] private CinemachineCamera matchCamera;
    [SerializeField] private Transform playerOneSpawnPoint;
    [SerializeField] private Transform playerTwoSpawnPoint;
    [SerializeField, Min(0.1f)] private float focusOrthographicSize = 2.5f;
    [SerializeField, Min(0f)] private float transitionDamping = 0.15f;

    private CinemachineTargetGroup targetGroup;
    private MatchCameraDirector cameraDirector;
    private TargetGroup legacyTargetGroup;
    private CinemachineGroupFraming groupFraming;
    private CinemachineFollow cameraFollow;
    private CinemachineConfiner2D confiner;
    private Transform focusAnchor;
    private CharacterCoordinator focusedPlayer;
    private Transform focusedSpawnPoint;
    private CameraTarget authoredTarget;
    private LensSettings authoredLens;
    private TrackerSettings authoredTrackerSettings;
    private Vector3 authoredFollowOffset;
    private float authoredFramingDamping;
    private bool authoredFramingEnabled;
    private bool introActive;
    private bool returningToMatch;

    private void Awake()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        cameraDirector = GetComponent<MatchCameraDirector>();
        legacyTargetGroup = GetComponent<TargetGroup>();
        groupFraming = matchCamera != null ? matchCamera.GetComponent<CinemachineGroupFraming>() : null;
        cameraFollow = matchCamera != null ? matchCamera.GetComponent<CinemachineFollow>() : null;
        confiner = matchCamera != null ? matchCamera.GetComponent<CinemachineConfiner2D>() : null;
        if (matchCamera == null || groupFraming == null || cameraFollow == null)
            return;

        authoredTarget = matchCamera.Target;
        authoredLens = matchCamera.Lens;
        authoredTrackerSettings = cameraFollow.TrackerSettings;
        authoredFollowOffset = cameraFollow.FollowOffset;
        authoredFramingDamping = groupFraming.Damping;
        authoredFramingEnabled = groupFraming.enabled;

        GameObject anchor = new GameObject("Countdown Camera Focus");
        anchor.hideFlags = HideFlags.HideInHierarchy;
        focusAnchor = anchor.transform;
    }

    private void Update()
    {
        if (introActive && !returningToMatch)
            UpdateFocusAnchor();
    }

    public void FocusOnPlayer(PlayerSlot slot)
    {
        if (matchCamera == null || groupFraming == null || cameraFollow == null || focusAnchor == null)
        {
            Debug.LogError("[CountdownCameraFocus] Assign a Cinemachine camera with Follow and Group Framing.", this);
            return;
        }

        RefreshNormalTargets();
        focusedPlayer = FindPlayer(slot);
        focusedSpawnPoint = slot == PlayerSlot.PlayerOne ? playerOneSpawnPoint : playerTwoSpawnPoint;
        if (focusedPlayer == null || focusedSpawnPoint == null)
        {
            Debug.LogWarning($"[CountdownCameraFocus] Missing player or spawn point for {slot}.", this);
            ReturnToMatch();
            return;
        }

        introActive = true;
        returningToMatch = false;
        UpdateFocusAnchor();

        groupFraming.enabled = false;
        matchCamera.Target = new CameraTarget
        {
            TrackingTarget = focusAnchor,
            LookAtTarget = focusAnchor,
            CustomLookAtTarget = false
        };

        cameraFollow.FollowOffset = new Vector3(0f, 0f, authoredFollowOffset.z);
        TrackerSettings tracker = authoredTrackerSettings;
        tracker.PositionDamping = new Vector3(transitionDamping, transitionDamping,
            authoredTrackerSettings.PositionDamping.z);
        cameraFollow.TrackerSettings = tracker;

        LensSettings lens = authoredLens;
        lens.OrthographicSize = focusOrthographicSize;
        matchCamera.Lens = lens;
        confiner?.InvalidateLensCache();
    }

    public void ReturnToMatch()
    {
        if (!introActive)
            return;

        returningToMatch = true;
        focusedPlayer = null;
        focusedSpawnPoint = null;
        matchCamera.Target = authoredTarget;
        matchCamera.Lens = authoredLens;
        confiner?.InvalidateLensCache();
        cameraFollow.FollowOffset = authoredFollowOffset;
        cameraFollow.TrackerSettings = authoredTrackerSettings;
        groupFraming.enabled = authoredFramingEnabled;
        groupFraming.Damping = transitionDamping;
    }

    public void EndIntro()
    {
        if (!introActive)
            return;

        ReturnToMatch();
        groupFraming.Damping = authoredFramingDamping;
        introActive = false;
        returningToMatch = false;
    }

    private void OnDisable()
    {
        EndIntro();
    }

    private void OnDestroy()
    {
        if (focusAnchor != null)
        {
            if (Application.isPlaying)
                Destroy(focusAnchor.gameObject);
            else
                DestroyImmediate(focusAnchor.gameObject);
        }
    }

    private void UpdateFocusAnchor()
    {
        if (focusedPlayer == null || focusedSpawnPoint == null)
            return;

        float visualCenterY = focusedPlayer.Sprite != null
            ? focusedPlayer.Sprite.bounds.center.y
            : focusedPlayer.transform.position.y;
        focusAnchor.position = new Vector3(focusedSpawnPoint.position.x, visualCenterY,
            focusedPlayer.transform.position.z);
    }

    private CharacterCoordinator FindPlayer(PlayerSlot slot)
    {
        foreach (CinemachineTargetGroup.Target target in targetGroup.Targets)
        {
            if (target.Object == null)
                continue;

            CharacterCoordinator player = target.Object.GetComponent<CharacterCoordinator>();
            if (player != null && player.Slot == slot && player.Health != null &&
                player.Health.ShouldCameraTrack)
                return player;
        }

        return null;
    }

    private void RefreshNormalTargets()
    {
        if (cameraDirector != null)
            cameraDirector.RefreshTargets(true);
        else if (legacyTargetGroup != null)
            legacyTargetGroup.RefreshTargets(true);
    }
}
