using System;
using UnityEngine;

public struct CollisionPairKey : IEquatable<CollisionPairKey>
{
    private int firstId;
    private int secondId;

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
