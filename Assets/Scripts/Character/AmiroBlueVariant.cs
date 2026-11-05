using UnityEngine;

/// <summary>Changes Amiro's red sprite palette to blue on a single spawned instance.</summary>
[DisallowMultipleComponent]
public sealed class AmiroBlueVariant : MonoBehaviour
{
    [SerializeField] private Material blueMaterial;

    public void Apply()
    {
        if (blueMaterial == null)
        {
            Debug.LogError("[AmiroBlueVariant] Blue palette material is missing.", this);
            return;
        }

        // The Animator and guard effects keep their existing sprites and timing.
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
            renderer.sharedMaterial = blueMaterial;
    }
}
