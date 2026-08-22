using UnityEngine;

public class InstaKillZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // Direct check: CharacterHealth must be on the same object as the collider
        CharacterHealth health = other.GetComponent<CharacterHealth>();
        if (health != null)
        {
            health.FallPenalty();
            return;
        }

        // Destroy projectiles and other loose objects (but not character child hitboxes)
        if (other.GetComponentInParent<CharacterHealth>() == null)
            Destroy(other.gameObject);
    }
}
