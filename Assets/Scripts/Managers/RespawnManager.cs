using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject playerAiPrefab;

    [Header("SpawnPoints")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;

    [HideInInspector] public GameObject p1Instance;
    [HideInInspector] public GameObject p2Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitialSpawn();
    }

    private void InitialSpawn()
    {
        if (playerPrefab != null && p1SpawnPoint != null)
        {
            p1Instance = Instantiate(playerPrefab, p1SpawnPoint.position, Quaternion.identity);
        }

        if (playerAiPrefab != null && p2SpawnPoint != null)
        {
            p2Instance = Instantiate(playerAiPrefab, p2SpawnPoint.position, Quaternion.identity);

            Vector3 aiScale = p2Instance.transform.localScale;
            aiScale.x = -Mathf.Abs(aiScale.x);
            p2Instance.transform.localScale = aiScale;
        }
    }

    public void RespawnPlayerAfterFall(PlayerHealth healthScript, int playerIndex)
    {
        Transform targetSpawn = (playerIndex == 0) ? p1SpawnPoint : p2SpawnPoint;

        healthScript.transform.position = targetSpawn.position;

        Rigidbody2D rb = healthScript.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void ResetRoundPositionsAndHealth()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            PlayerHealth pHealth = p.GetComponent<PlayerHealth>();

            if (p.PlayerIndex == 0)
            {
                if (p1SpawnPoint != null)
                {
                    p.transform.position = p1SpawnPoint.position;
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
            else
            {
                if (p2SpawnPoint != null)
                {
                    p.transform.position = p2SpawnPoint.position;
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
        }
    }
}
