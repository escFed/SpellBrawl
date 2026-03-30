using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int index = playerInput.playerIndex;

        if (index < spawnPoints.Length)
        {
            playerInput.transform.SetPositionAndRotation(spawnPoints[index].position, spawnPoints[index].rotation);
        }
    }
}
