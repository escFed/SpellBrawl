using UnityEngine;

// Owned by CharacterMovement or Dummy. Physics retains gravity; this tracks only launch momentum.
public sealed class KnockbackMotion
{
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
    private Vector2 ordinaryVelocity;
    public Vector2 Velocity { get; private set; }
    public bool IsActive => Velocity.sqrMagnitude > 0.000001f;

    public void Launch(Rigidbody2D body, Vector2 velocity)
    {
        ordinaryVelocity = Vector2.zero;
        Velocity = velocity;
        body.linearVelocity = velocity;
    }

    public void Clear(Rigidbody2D body)
    {
        ordinaryVelocity = Vector2.zero;
        Velocity = Vector2.zero;
        // Static bodies have no velocity to clear during death/respawn cleanup.
        if (body != null && body.bodyType != RigidbodyType2D.Static)
            body.linearVelocity = Vector2.zero;
    }

    public void SetOrdinaryVelocity(Rigidbody2D body, Vector2 velocity)
    {
        ordinaryVelocity = velocity;
        body.linearVelocity = velocity + Velocity;
    }

    public void Step(Rigidbody2D body, float deltaTime, float airDeceleration, float groundDeceleration)
    {
        if (body == null)
            return;
        if (!body.simulated || body.bodyType != RigidbodyType2D.Dynamic)
        {
            Clear(body);
            return;
        }
        if (!IsActive)
            return;

        bool grounded = false;
        int count = body.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            ContactPoint2D contact = contacts[i];
            Collider2D surface = contact.collider.attachedRigidbody == body ? contact.otherCollider : contact.collider;
            // Characters should not become floors or walls for launch friction.
            if (surface == null || (surface.attachedRigidbody != null &&
                surface.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic))
                continue;

            Vector2 normal = contact.normal;
            Velocity = KnockbackCalculation.RemoveIntoSurface(Velocity, normal);
            ordinaryVelocity = KnockbackCalculation.RemoveIntoSurface(ordinaryVelocity, normal);
            body.linearVelocity = KnockbackCalculation.RemoveIntoSurface(body.linearVelocity, normal);
            // Physics friction has already reduced the total tangential velocity. Attribute that
            // reduction to both components before decay, otherwise it becomes reverse momentum.
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            float expected = Vector2.Dot(Velocity + ordinaryVelocity, tangent);
            float actual = Vector2.Dot(body.linearVelocity, tangent);
            if (Mathf.Abs(expected) > 0.0001f)
            {
                float retained = Mathf.Clamp01(actual / expected);
                Velocity += tangent * (Vector2.Dot(Velocity, tangent) * (retained - 1f));
                ordinaryVelocity += tangent * (Vector2.Dot(ordinaryVelocity, tangent) * (retained - 1f));
            }
            grounded |= normal.y > 0.5f && body.linearVelocity.y <= 0.01f;
        }

        ordinaryVelocity = body.linearVelocity - Velocity;
        float deceleration = grounded ? groundDeceleration : airDeceleration;
        Velocity = Vector2.MoveTowards(Velocity, Vector2.zero, Mathf.Max(0f, deceleration) * Mathf.Max(0f, deltaTime));
        body.linearVelocity = ordinaryVelocity + Velocity;
    }
}
