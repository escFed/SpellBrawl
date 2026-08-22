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

    private void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = 0.5f;
            RefreshTargets(false);
        }
    }

    public void RefreshTargets(bool forceRefresh = false)
    {
        if (targetGroup == null) return;

        PlayerController[] alivePlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        if (forceRefresh || targetGroup.Targets.Count != alivePlayers.Length)
        {
            targetGroup.Targets.Clear();

            foreach (PlayerController p in alivePlayers)
            {
                targetGroup.AddMember(p.transform, 1f, 3f);
            }
        }
    }
}