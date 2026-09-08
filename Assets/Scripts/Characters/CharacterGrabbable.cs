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


    public bool CanBeGrabbed => controller != null && health != null && !controller.IsDead &&
        !health.IsRespawnProtected && !isGrabbed;
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
        controller.Movement.ResetKnockback();
        controller.Shield?.Break();
        controller.CancelGroundJumpAvailability();
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



    public void OnThrown(CombatHit hit)
    {
        isGrabbed = false;
        RestorePhysics();
        controller.controlsEnabled = true;
        health.ReceiveHit(hit);
    }

    public void OnReleased()
    {
        isGrabbed = false;
        controller.Movement.ResetKnockback();
        RestorePhysics();
        controller.controlsEnabled = true;
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
