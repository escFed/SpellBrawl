using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRoll : MonoBehaviour
{
    [Header("Roll Usage")]
    [SerializeField, Min(1)] private int maxRolls = 3;
    [SerializeField, Min(0f)] private float cooldownDuration = 5f;

    public bool CanRoll => usage != null && usage.CanUse;
    public int RemainingRolls => usage?.RemainingUses ?? 0;
    public float CooldownRemaining => usage?.CooldownRemaining ?? 0f;

    private LimitedUseCooldown usage;
    private Rigidbody2D body;
    private CharacterMovement movement;
    private readonly CollisionIgnoreScope characterCollisions = new CollisionIgnoreScope();
    private Vector2 lastSafeCollisionPosition;
    private bool hasSafeCollisionPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<CharacterMovement>();
        InitializeUsage();
    }

    private void Update()
    {
        usage.Tick(Time.deltaTime);
    }

    public bool TryStartRoll() => usage.TryConsume();

    public void CompleteRoll() => usage.CompleteUse();

    public void ResetRolls() => usage.Reset();

    public void BeginCharacterCollisionPassThrough()
    {
        EndCharacterCollisionPassThrough();

        Collider2D[] ownerColliders = GetPhysicalColliders(gameObject);
        PlayerController[] characters = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Dummy[] trainingDummies = FindObjectsByType<Dummy>(FindObjectsSortMode.None);

        foreach (PlayerController character in characters)
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

    private void InitializeUsage()
    {
        usage = new LimitedUseCooldown(Mathf.Max(1, maxRolls), Mathf.Max(0f, cooldownDuration));
    }

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

internal sealed class CollisionIgnoreScope
{
    private static readonly Dictionary<CollisionPairKey, CollisionIgnoreEntry> ActivePairs =
        new Dictionary<CollisionPairKey, CollisionIgnoreEntry>();

    private readonly HashSet<CollisionPairKey> ownedPairs = new HashSet<CollisionPairKey>();

    public bool IsEmpty => ownedPairs.Count == 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSharedState() => ActivePairs.Clear();

    public void Ignore(Collider2D first, Collider2D second)
    {
        if (first == null || second == null || first == second)
            return;

        CollisionPairKey key = new CollisionPairKey(first, second);
        if (!ownedPairs.Add(key))
            return;

        if (!ActivePairs.TryGetValue(key, out CollisionIgnoreEntry entry))
        {
            bool wasAlreadyIgnored = Physics2D.GetIgnoreCollision(first, second);
            entry = new CollisionIgnoreEntry(first, second, wasAlreadyIgnored);
            ActivePairs.Add(key, entry);

            if (!wasAlreadyIgnored)
                Physics2D.IgnoreCollision(first, second, true);
        }

        entry.UserCount++;
    }

    public bool HasOverlap()
    {
        foreach (CollisionPairKey key in ownedPairs)
        {
            if (ActivePairs.TryGetValue(key, out CollisionIgnoreEntry entry) && entry.IsOverlapping())
                return true;
        }

        return false;
    }

    public bool HasOverlapThatWillBeRestored()
    {
        foreach (CollisionPairKey key in ownedPairs)
        {
            if (ActivePairs.TryGetValue(key, out CollisionIgnoreEntry entry) &&
                entry.UserCount == 1 && !entry.WasAlreadyIgnored && entry.IsOverlapping())
                return true;
        }

        return false;
    }

    public void RestoreAll()
    {
        foreach (CollisionPairKey key in ownedPairs)
        {
            if (!ActivePairs.TryGetValue(key, out CollisionIgnoreEntry entry))
                continue;

            entry.UserCount--;
            if (entry.UserCount > 0)
                continue;

            if (!entry.WasAlreadyIgnored && entry.First != null && entry.Second != null)
                Physics2D.IgnoreCollision(entry.First, entry.Second, false);

            ActivePairs.Remove(key);
        }

        ownedPairs.Clear();
    }

    private readonly struct CollisionPairKey : IEquatable<CollisionPairKey>
    {
        private readonly int firstId;
        private readonly int secondId;

        public CollisionPairKey(Collider2D first, Collider2D second)
        {
            int firstInstanceId = first.GetInstanceID();
            int secondInstanceId = second.GetInstanceID();
            firstId = Mathf.Min(firstInstanceId, secondInstanceId);
            secondId = Mathf.Max(firstInstanceId, secondInstanceId);
        }

        public bool Equals(CollisionPairKey other) => firstId == other.firstId && secondId == other.secondId;
        public override bool Equals(object obj) => obj is CollisionPairKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(firstId, secondId);
    }

    private sealed class CollisionIgnoreEntry
    {
        public Collider2D First { get; }
        public Collider2D Second { get; }
        public bool WasAlreadyIgnored { get; }
        public int UserCount { get; set; }

        public CollisionIgnoreEntry(Collider2D first, Collider2D second, bool wasAlreadyIgnored)
        {
            First = first;
            Second = second;
            WasAlreadyIgnored = wasAlreadyIgnored;
        }

        public bool IsOverlapping()
        {
            if (First == null || Second == null || !First.enabled || !Second.enabled ||
                !First.gameObject.activeInHierarchy || !Second.gameObject.activeInHierarchy)
                return false;

            return Physics2D.Distance(First, Second).isOverlapped;
        }
    }
}
