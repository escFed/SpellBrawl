using System.Collections.Generic;
using UnityEngine;

// Added to spawned damaging effects, so AI never needs to scan the scene for hazards.
public class AIDangerSource : MonoBehaviour
{
    private static List<AIDangerSource> active = new List<AIDangerSource>();
    private Transform owner;
    private Transform target;
    private Rigidbody2D body;
    private Vector2 previousPosition;
    private Vector2 estimatedVelocity;
    private float radius;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => active.Clear();

    public static void Attach(GameObject effect, GameObject caster, float dangerRadius, Transform homingTarget = null)
    {
        if (effect == null)
            return;
        AIDangerSource source = effect.GetComponent<AIDangerSource>();
        if (source == null)
            source = effect.AddComponent<AIDangerSource>();
        source.owner = caster != null ? caster.transform.root : null;
        source.target = homingTarget;
        source.body = effect.GetComponent<Rigidbody2D>();
        source.previousPosition = effect.transform.position;
        source.radius = dangerRadius;
        if (!active.Contains(source))
            active.Add(source);
    }

    public static bool IsIncoming(Transform self, Vector2 selfVelocity)
    {
        if (self == null)
            return false;
        for (int i = 0; i < active.Count; i++)
        {
            AIDangerSource source = active[i];
            if (source == null || !source.enabled || !source.gameObject.activeInHierarchy ||
                source.owner == null || source.owner == self.root)
                continue;

            Vector2 displacement = (Vector2)source.transform.position - (Vector2)self.position;
            Vector2 velocity = source.GetVelocity() - selfVelocity;
            float speedSquared = velocity.sqrMagnitude;
            float closestTime = speedSquared > 0.01f
                ? Mathf.Clamp(-Vector2.Dot(displacement, velocity) / speedSquared, 0f, 0.7f)
                : 0f;
            float safeRadius = source.radius + 0.65f;
            if ((displacement + velocity * closestTime).sqrMagnitude <= safeRadius * safeRadius)
                return true;
        }
        return false;
    }

    private Vector2 GetVelocity()
    {
        if (body != null && body.linearVelocity.sqrMagnitude > 0.01f)
            return body.linearVelocity;
        if (target != null)
            return ((Vector2)(target.position - transform.position)).normalized *
                Mathf.Max(estimatedVelocity.magnitude, 7f);
        return estimatedVelocity;
    }

    private void OnEnable()
    {
        if (!active.Contains(this))
            active.Add(this);
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (Time.deltaTime > 0f)
            estimatedVelocity = ((Vector2)transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }

    private void OnDisable() => active.Remove(this);
    private void OnDestroy() => active.Remove(this);
}
