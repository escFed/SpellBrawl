using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerRespawn player = other.GetComponent<PlayerRespawn>();

        if (player != null)
        {
            player.Respawn();
        }
    }
}