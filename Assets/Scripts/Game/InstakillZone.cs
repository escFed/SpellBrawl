using System.Collections;
using UnityEngine;

public class InstaKillZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // Unity also sends exits when colliders/objects are disabled or scenes unload.
        // Only leaving an active boundary with an active collider counts as a fall.
        if (!gameObject.activeSelf || !isActiveAndEnabled || other == null || !other.enabled || !other.gameObject.activeInHierarchy)
            return;

        // Unity can report this exit immediately before applying a disable. Confirm it
        // one frame later so disabling the boundary cannot consume a stock.
        StartCoroutine(ConfirmActiveExit(other));
    }

    private IEnumerator ConfirmActiveExit(Collider2D other)
    {
        yield return null;

        if (!gameObject.activeSelf || !isActiveAndEnabled || other == null || !other.enabled || !other.gameObject.activeInHierarchy)
            yield break;

        // Direct check: CharacterHealth must be on the same object as the collider.
        CharacterHealth health = other.GetComponent<CharacterHealth>();
        if (health != null)
        {
            health.FallPenalty();
            yield break;
        }

        // Destroy projectiles and other loose objects (but not character child hitboxes)
        if (other.GetComponentInParent<CharacterHealth>() == null)
            Destroy(other.gameObject);
    }
}
