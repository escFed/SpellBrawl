using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject playerAiPrefab;

    [Header("SpawnPoint")]
    public Transform spawnPointPlayer1;
    public Transform spawnPointPlayer2;

    private void Start()
    {
        if (playerPrefab != null && spawnPointPlayer1 != null)
        {
            Instantiate(playerPrefab, spawnPointPlayer1.position, Quaternion.identity);
        }

        if (playerAiPrefab != null && spawnPointPlayer2 != null)
        {
            Instantiate(playerAiPrefab, spawnPointPlayer2.position, Quaternion.identity);

            Vector3 aiScale = playerAiPrefab.transform.localScale;
            aiScale.x = -Mathf.Abs(aiScale.x);
        }
    }
}
