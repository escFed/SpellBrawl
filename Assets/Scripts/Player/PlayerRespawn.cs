using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 spawnPoint;

    private void Start()
    {
        spawnPoint = transform.position;
    }

    public void Respawn()
    {
        transform.position = spawnPoint;

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
