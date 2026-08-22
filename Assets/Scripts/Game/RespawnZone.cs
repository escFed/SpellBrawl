using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterHealth health = other.GetComponentInParent<CharacterHealth>();

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