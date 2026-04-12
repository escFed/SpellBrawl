using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            if (other.gameObject == health.gameObject)
            {
                health.FallPenalty();
            }
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}