using UnityEngine;

public class CollisionIgnoreEntry
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
