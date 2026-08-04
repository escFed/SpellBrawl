using System.Collections.Generic;
using UnityEngine;

public class CharacterGrab : MonoBehaviour
{
    [Header("Grab Settings")]
    public Transform throwPoint;

    private PlayerController controller;
    private CharacterHitBox hitBox;
    private IGrabbable grabbedTarget;
    private List<CollisionPair> ignoredCollisionPairs = new List<CollisionPair>();

    public bool HasGrabbedTarget => grabbedTarget != null;

    private IInputProvider Input => controller.ActiveInput;
    private Transform HoldPoint => throwPoint != null ? throwPoint : transform;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        hitBox = GetComponent<CharacterHitBox>();
    }

    public StateCharacter ResolveGrabState()
    {
        Vector2 dir = Input != null ? Input.CurrentDirection : Vector2.zero;
        bool hasHorizontal = Mathf.Abs(dir.x) >= controller.stats.tiltThreshold;
        bool inputOppositeFacing = (dir.x > 0f && !hitBox.IsFacingRight) || (dir.x < 0f && hitBox.IsFacingRight);
        bool canPivotGrab = controller.stateMachine.CurrentState == controller.stateMachine.Move;

        Input?.ConsumeGrab();

        if (canPivotGrab && hasHorizontal && inputOppositeFacing)
            return StateCharacter.PivotGrab;

        return StateCharacter.Grab;
    }

    public bool TryCaptureTarget(IGrabbable target)
    {
        if (grabbedTarget != null || target == null || !target.CanBeGrabbed)
            return false;

        Transform targetTransform = target.GrabTransform;
        if (targetTransform == null || targetTransform.root == transform.root)
            return false;

        grabbedTarget = target;
        IgnoreTargetCollisions(targetTransform);
        grabbedTarget.OnGrabbed(HoldPoint);
        return true;
    }

    public void UpdateGrabbedTargetPosition()
    {
        grabbedTarget?.UpdateGrabbedPosition(HoldPoint);
    }

    public ThrowDirection ResolveThrowDirection()
    {
        Vector2 dir = Input != null ? Input.CurrentDirection : Vector2.zero;

        if (dir.y >= controller.stats.tiltThreshold)
            return ThrowDirection.Up;

        if (dir.y <= -controller.stats.tiltThreshold)
            return ThrowDirection.Down;

        bool back = (dir.x > controller.stats.tiltThreshold && !hitBox.IsFacingRight) || (dir.x < -controller.stats.tiltThreshold && hitBox.IsFacingRight);

        return back ? ThrowDirection.Back : ThrowDirection.Forward;
    }

    public ThrowStats GetThrowStats(ThrowDirection direction)
    {
        return direction switch
        {
            ThrowDirection.Back => controller.stats.backThrow,
            ThrowDirection.Up => controller.stats.upThrow,
            ThrowDirection.Down => controller.stats.downThrow,
            _ => controller.stats.forwardThrow
        };
    }

    public void ApplyThrow(ThrowDirection direction)
    {
        if (grabbedTarget == null)
            return;

        ThrowStats throwStats = GetThrowStats(direction);
        if (throwStats == null)
        {
            ReleaseGrabbedTarget();
            return;
        }

        IGrabbable target = grabbedTarget;
        grabbedTarget = null;
        RestoreTargetCollisions();
        target.OnThrown(throwStats.damage, GetDirectedThrowKnockback(direction, throwStats.knockback));
        GetComponent<EnergyManager>()?.AddEnergy(throwStats.energyGain);
    }

    public void ApplyPummel(GrabStats stats)
    {
        if (grabbedTarget == null || stats == null)
            return;

        grabbedTarget.TakePummelDamage(stats.pummelDamage);
        GetComponent<EnergyManager>()?.AddEnergy(stats.pummelEnergyGain);
    }

    public void ReleaseGrabbedTarget()
    {
        if (grabbedTarget == null)
            return;

        IGrabbable target = grabbedTarget;
        grabbedTarget = null;
        RestoreTargetCollisions();
        target.OnReleased();
    }

    public void SetupGrabbox() => hitBox.SetupGrabbox(this);
    public void SetupPivotGrabbox() => hitBox.SetupPivotGrabbox(this);
    public void OpenGrabbox() => hitBox.SetGrabbox(true);
    public void CloseGrabbox() => hitBox.SetGrabbox(false);
    public void OpenPivotGrabbox() => hitBox.SetPivotGrabbox(true);
    public void ClosePivotGrabbox() => hitBox.SetPivotGrabbox(false);

    private void IgnoreTargetCollisions(Transform targetTransform)
    {
        RestoreTargetCollisions();

        Collider2D[] ownerColliders = transform.root.GetComponentsInChildren<Collider2D>(true);
        Collider2D[] targetColliders = targetTransform.root.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider == null || ownerCollider.isTrigger)
                continue;

            foreach (Collider2D targetCollider in targetColliders)
            {
                if (targetCollider == null || targetCollider.isTrigger ||
                    Physics2D.GetIgnoreCollision(ownerCollider, targetCollider))
                    continue;

                Physics2D.IgnoreCollision(ownerCollider, targetCollider, true);
                ignoredCollisionPairs.Add(new CollisionPair(ownerCollider, targetCollider));
            }
        }
    }

    private void RestoreTargetCollisions()
    {
        foreach (CollisionPair pair in ignoredCollisionPairs)
        {
            if (pair.OwnerCollider != null && pair.TargetCollider != null)
                Physics2D.IgnoreCollision(pair.OwnerCollider, pair.TargetCollider, false);
        }

        ignoredCollisionPairs.Clear();
    }

    private Vector2 GetDirectedThrowKnockback(ThrowDirection direction, Vector2 baseKnockback)
    {
        float facingSign = hitBox.IsFacingRight ? 1f : -1f;

        return direction switch
        {
            ThrowDirection.Back => new Vector2(-Mathf.Abs(baseKnockback.x) * facingSign, baseKnockback.y),
            ThrowDirection.Up => new Vector2(0f, Mathf.Abs(baseKnockback.y)),
            ThrowDirection.Down => new Vector2(0f, -Mathf.Abs(baseKnockback.y)),
            _ => new Vector2(Mathf.Abs(baseKnockback.x) * facingSign, baseKnockback.y)
        };
    }

    private struct CollisionPair
    {
        public Collider2D OwnerCollider { get; }
        public Collider2D TargetCollider { get; }

        public CollisionPair(Collider2D ownerCollider, Collider2D targetCollider)
        {
            OwnerCollider = ownerCollider;
            TargetCollider = targetCollider;
        }
    }
}

