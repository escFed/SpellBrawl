using System.Collections.Generic;
using UnityEngine;

public class CharacterDash : MonoBehaviour
{
    [Header("Dash Usage")]
    [SerializeField, Min(1)] private int maxDashes = 3;

    public bool CanDash => usage != null && usage.CanUse;
    public int RemainingDashes => usage?.RemainingUses ?? 0;
    public float CooldownRemaining => usage?.CooldownRemaining ?? 0f;
    public float CooldownDuration => usage?.CooldownDuration ?? 0f;
    public float Direction { get; private set; } = 1f;

    private LimitedUseCooldown usage;
    private CharacterCoordinator controller;
    private Rigidbody2D body;
    private CharacterMovement movement;
    private readonly CollisionIgnoreScope characterCollisions = new CollisionIgnoreScope();
    private Vector2 lastSafeCollisionPosition;
    private bool hasSafeCollisionPosition;

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<CharacterMovement>();
        float cooldown = controller != null && controller.stats != null ? controller.stats.dashCooldown : 5f;
        usage = new LimitedUseCooldown(Mathf.Max(1, maxDashes), Mathf.Max(0f, cooldown));
    }

    private void Update()
    {
        usage.Tick(Time.deltaTime);
    }

    public bool TryStartDash(float horizontalDirection)
    {
        if (controller == null || controller.stats == null || !usage.TryConsume())
            return false;

        Direction = Mathf.Abs(horizontalDirection) >= 0.2f
            ? Mathf.Sign(horizontalDirection)
            : Mathf.Sign(transform.localScale.x);
        if (Mathf.Approximately(Direction, 0f))
            Direction = 1f;
        controller.Combat.FaceDirection(Direction);
        return true;
    }

    public void CompleteDash() => usage?.CompleteUse();

    public void ResetDash() => usage?.Reset();

    public void BeginCharacterCollisionPassThrough()
    {
        EndCharacterCollisionPassThrough();

        Collider2D[] ownerColliders = GetPhysicalColliders(gameObject);
        CharacterCoordinator[] characters = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        Dummy[] trainingDummies = FindObjectsByType<Dummy>(FindObjectsSortMode.None);

        foreach (CharacterCoordinator character in characters)
        {
            if (character != null && character.gameObject != gameObject)
                IgnoreBodyCollisions(ownerColliders, character.gameObject);
        }

        foreach (Dummy trainingDummy in trainingDummies)
        {
            if (trainingDummy != null && trainingDummy.gameObject != gameObject)
                IgnoreBodyCollisions(ownerColliders, trainingDummy.gameObject);
        }

        hasSafeCollisionPosition = false;
        TrackSafeCollisionPosition();
    }

    public void TrackSafeCollisionPosition()
    {
        if (body == null || characterCollisions.IsEmpty || characterCollisions.HasOverlap())
            return;

        lastSafeCollisionPosition = body.position;
        hasSafeCollisionPosition = true;
    }

    public void EndCharacterCollisionPassThrough()
    {
        if (characterCollisions.IsEmpty)
            return;

        if (hasSafeCollisionPosition && characterCollisions.HasOverlapThatWillBeRestored() && body != null)
        {
            body.position = lastSafeCollisionPosition;
            if (movement != null)
                movement.StopHorizontalMovement();
            else
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            Physics2D.SyncTransforms();
        }

        characterCollisions.RestoreAll();
        hasSafeCollisionPosition = false;
    }

    private void OnDisable() => EndCharacterCollisionPassThrough();
    private void OnDestroy() => EndCharacterCollisionPassThrough();

    private void IgnoreBodyCollisions(Collider2D[] ownerColliders, GameObject target)
    {
        Collider2D[] targetColliders = GetPhysicalColliders(target);

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            foreach (Collider2D targetCollider in targetColliders)
                characterCollisions.Ignore(ownerCollider, targetCollider);
        }
    }

    private static Collider2D[] GetPhysicalColliders(GameObject target)
    {
        Collider2D[] colliders = target.GetComponents<Collider2D>();
        List<Collider2D> physicalColliders = new List<Collider2D>(colliders.Length);

        foreach (Collider2D collider in colliders)
        {
            if (collider != null && !collider.isTrigger)
                physicalColliders.Add(collider);
        }

        return physicalColliders.ToArray();
    }
}
