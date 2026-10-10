using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionIgnoreScope
{
    private static Dictionary<CollisionPairKey, CollisionIgnoreEntry> ActivePairs = new Dictionary<CollisionPairKey, CollisionIgnoreEntry>();

    private HashSet<CollisionPairKey> ownedPairs = new HashSet<CollisionPairKey>();

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
}
