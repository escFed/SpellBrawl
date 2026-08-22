using UnityEngine;

public class GrabHitbox : MonoBehaviour
{
    private CharacterGrab ownerGrab;
    private Collider2D grabCollider;
    private bool hasGrabbed;

    private void Awake()
    {
        grabCollider = GetComponent<Collider2D>();

        if (grabCollider != null)
            grabCollider.enabled = false;
    }

    public void Setup(CharacterGrab grab)
    {
        ownerGrab = grab;
        hasGrabbed = false;
    }

    public void BeginGrab()
    {
        hasGrabbed = false;

        if (grabCollider != null)
            grabCollider.enabled = true;
    }

    public void EndGrab()
    {
        if (grabCollider != null)
            grabCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasGrabbed || ownerGrab == null)
            return;

        if (other.transform.root == transform.root)
            return;

        IGrabbable target = other.GetComponentInParent<IGrabbable>();
        if (target == null || !target.CanBeGrabbed)
            return;

        hasGrabbed = ownerGrab.TryCaptureTarget(target);
    }
}
