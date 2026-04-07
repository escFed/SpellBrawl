using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 spawnPoint;
    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        spawnPoint = transform.position;
    }

    public void Respawn()
    {
        if (controller != null)
        {
            controller.Respawn(spawnPoint);
        }
        else
        {
            transform.position = spawnPoint;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
}
