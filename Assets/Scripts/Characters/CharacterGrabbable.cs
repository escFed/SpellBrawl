using Unity.VisualScripting;
using UnityEngine;

public class CharacterGrabbable : MonoBehaviour, IGrabbable
{
    private PlayerController controller;
    private CharacterHealth health;
    private Rigidbody2D body;
    private RigidbodyType2D originalBodyType;
    private float originalGravityScale;
    private bool bodyStateStored;
    private bool isGrabbed;
    private bool stopHorizontalThrowOnLanding;

    public bool CanBeGrabbed => controller != null && !controller.IsDead && !isGrabbed;
    public Transform GrabTransform => transform;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<CharacterHealth>();
        body = GetComponent<Rigidbody2D>();
    }

    public void OnGrabbed(Transform holdPoint)
    {
        if (!CanBeGrabbed)
            return;

        isGrabbed = true;
        stopHorizontalThrowOnLanding = false;
        controller.Shield?.Break();
        controller.ChangeState(StateCharacter.Idle);
        controller.controlsEnabled = false;
        controller.Movement.StopAllMovement();
        controller.ActiveInput?.ClearAllInputs();
        SuspendPhysics();
        UpdateGrabbedPosition(holdPoint);
    }

    public void UpdateGrabbedPosition(Transform holdPoint)
    {
        if (!isGrabbed || holdPoint == null)
            return;

        if (body != null)
        {
            body.position = holdPoint.position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        else
        {
            transform.position = holdPoint.position;
        }
    }

    public void TakePummelDamage(int amount)
    {
        health.TakePummelDamage(amount);
    }

    public void OnThrown(int amount, Vector2 knockback)
    {
        isGrabbed = false;
        stopHorizontalThrowOnLanding = Mathf.Abs(knockback.x) > 0.01f;
        RestorePhysics();
        controller.controlsEnabled = true;
        health.TakeDamage(amount, knockback);
    }

    public void OnReleased()
    {
        isGrabbed = false;
        stopHorizontalThrowOnLanding = false;
        RestorePhysics();
        controller.controlsEnabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopThrowMovementOnGroundContact(collision);
    }

    private void StopThrowMovementOnGroundContact(Collision2D collision)
    {
        if (!stopHorizontalThrowOnLanding || body == null || body.linearVelocity.y > 0.01f)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < 0.5f)
                continue;

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            stopHorizontalThrowOnLanding = false;
            return;
        }
    }

    private void SuspendPhysics()
    {
        if (body == null || bodyStateStored)
            return;

        originalBodyType = body.bodyType;
        originalGravityScale = body.gravityScale;
        bodyStateStored = true;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    private void RestorePhysics()
    {
        if (body == null || !bodyStateStored)
            return;

        body.bodyType = originalBodyType;
        body.gravityScale = originalGravityScale;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        bodyStateStored = false;
    }
}
