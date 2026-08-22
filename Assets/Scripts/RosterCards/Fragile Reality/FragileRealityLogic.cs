using UnityEngine;

public class FragileRealityLogic : MonoBehaviour
{
    [SerializeField] private float platformDuration = 3f;
    [SerializeField] private GameObject platformPrefab;

    private PlayerController caster;
    private PlayerController target;

    // Método para inicializar desde la carta
    public void Initialize(PlayerController casterPlayer, PlayerController targetPlayer)
    {
        caster = casterPlayer;
        target = targetPlayer;
    }

    public void ActivateFragileReality()
    {
        if (caster == null || target == null)
        {
            Debug.LogError("❌ FragileRealityLogic no tiene caster o target asignados.");
            return;
        }

        // Spawn la plataforma en relación al target
        Vector3 offset = caster.transform.position - target.transform.position;
        Vector3 spawnPosition = target.transform.position + offset;

        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        Destroy(platform, platformDuration);
    }
}
