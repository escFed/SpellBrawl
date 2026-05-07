using UnityEngine;

public class InstaKillZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        CharacterHealth health = other.GetComponentInParent<CharacterHealth>();

        if (health != null)
        {
            if (other.gameObject == health.gameObject)
            {
                health.InstantGameOver();
            }
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}
