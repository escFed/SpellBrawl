using UnityEngine;
using Unity.Cinemachine;

public class TargetGroup : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;
    private float checkTimer = 0f;

    private void Awake()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        RefreshTargets(true);
    }

    private void LateUpdate()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = 0.5f;
            RefreshTargets(false);
            return;
        }

        SyncTargetWeights();
    }

    public void RefreshTargets(bool forceRefresh = false)
    {
        if (targetGroup == null) return;

        CharacterCoordinator[] players = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);

        if (forceRefresh || !TargetsMatch(players))
        {
            targetGroup.Targets.Clear();

            foreach (CharacterCoordinator player in players)
            {
                targetGroup.AddMember(player.transform, GetCameraWeight(player), 3f);
            }

            return;
        }

        SyncTargetWeights();
    }

    private bool TargetsMatch(CharacterCoordinator[] players)
    {
        if (targetGroup.Targets.Count != players.Length)
            return false;

        foreach (CharacterCoordinator player in players)
            if (player == null || targetGroup.FindMember(player.transform) < 0)
                return false;

        return true;
    }

    private void SyncTargetWeights()
    {
        if (targetGroup == null)
            return;

        foreach (CinemachineTargetGroup.Target target in targetGroup.Targets)
        {
            CharacterCoordinator player = target.Object != null
                ? target.Object.GetComponent<CharacterCoordinator>()
                : null;
            target.Weight = GetCameraWeight(player);
        }
    }

    private static float GetCameraWeight(CharacterCoordinator player)
    {
        return player != null && player.Health != null && player.Health.ShouldCameraTrack ? 1f : 0f;
    }
}
