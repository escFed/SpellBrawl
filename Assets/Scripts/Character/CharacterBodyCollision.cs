using System.Collections.Generic;
using UnityEngine;

// Body contact is handled separately from attack triggers. Players never exchange
// physics impulses; only horizontal approaches are limited by the movement rule.
[DefaultExecutionOrder(500)]
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class CharacterBodyCollision : MonoBehaviour
{
    private const float MinimumSideOverlap = 0.05f;
    private const float ContactSkin = 0.005f;
    private const float PassThroughOverlap = 0.02f;
    private static readonly List<CharacterBodyCollision> ActiveBodies = new List<CharacterBodyCollision>();

    private Rigidbody2D body;
    private CapsuleCollider2D bodyCollider;
    private CharacterDash dash;
    private LayerMask originalForceSendLayers;
    private LayerMask originalForceReceiveLayers;
    private bool hasOriginalForceLayers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveBodies() => ActiveBodies.Clear();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Start() => dash = GetComponent<CharacterDash>();

    private void OnEnable()
    {
        if (bodyCollider == null)
            return;

        // These masks affect only the root body collider. Attack and grab triggers
        // keep their own collision settings and continue detecting hits.
        originalForceSendLayers = bodyCollider.forceSendLayers;
        originalForceReceiveLayers = bodyCollider.forceReceiveLayers;
        hasOriginalForceLayers = true;
        int characterLayers = LayerMask.GetMask("Player", "Opponent");
        bodyCollider.forceSendLayers = originalForceSendLayers.value & ~characterLayers;
        bodyCollider.forceReceiveLayers = originalForceReceiveLayers.value & ~characterLayers;
        ActiveBodies.Add(this);
    }

    private void OnDisable()
    {
        ActiveBodies.Remove(this);
        if (bodyCollider == null || !hasOriginalForceLayers)
            return;

        bodyCollider.forceSendLayers = originalForceSendLayers;
        bodyCollider.forceReceiveLayers = originalForceReceiveLayers;
        hasOriginalForceLayers = false;
    }

    private void FixedUpdate()
    {
        if (!CanBlockSides() || dash != null && dash.IsPassingThroughCharacters)
            return;

        // All CharacterCoordinator.FixedUpdate calls finish before this component runs.
        // Each pair is resolved once, using both players' final intended velocities.
        for (int i = 0; i < ActiveBodies.Count; i++)
        {
            CharacterBodyCollision other = ActiveBodies[i];
            if (other == null || other == this || GetInstanceID() > other.GetInstanceID() ||
                !other.CanBlockSides() || other.dash != null && other.dash.IsPassingThroughCharacters)
                continue;

            ResolveSideApproach(this, other);
        }
    }

    private bool CanBlockSides()
    {
        return body != null && body.simulated && body.bodyType == RigidbodyType2D.Dynamic &&
            bodyCollider != null && bodyCollider.enabled;
    }

    private static void ResolveSideApproach(CharacterBodyCollision first, CharacterBodyCollision second)
    {
        Bounds a = first.bodyCollider.bounds;
        Bounds b = second.bodyCollider.bounds;
        float verticalOverlap = Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y);
        if (verticalOverlap < MinimumSideOverlap)
            return;

        CharacterBodyCollision left = a.center.x <= b.center.x ? first : second;
        CharacterBodyCollision right = left == first ? second : first;
        Bounds leftBounds = left.bodyCollider.bounds;
        Bounds rightBounds = right.bodyCollider.bounds;
        float gap = rightBounds.min.x - leftBounds.max.x;

        // When bodies are already overlapping (a vertical pass-through or a dash),
        // let them leave the overlap naturally instead of trapping either player.
        if (gap < -PassThroughOverlap)
            return;

        float leftSpeed = left.body.linearVelocity.x;
        float rightSpeed = right.body.linearVelocity.x;
        float closingSpeed = leftSpeed - rightSpeed;
        float allowedClosingSpeed = Mathf.Max(0f, gap - ContactSkin) / Time.fixedDeltaTime;
        if (closingSpeed <= allowedClosingSpeed)
            return;

        if (leftSpeed > 0f && rightSpeed < 0f)
        {
            float fraction = allowedClosingSpeed / closingSpeed;
            leftSpeed *= fraction;
            rightSpeed *= fraction;
        }
        else if (leftSpeed > rightSpeed && leftSpeed > 0f)
        {
            leftSpeed = rightSpeed + allowedClosingSpeed;
        }
        else
        {
            rightSpeed = leftSpeed - allowedClosingSpeed;
        }

        Vector2 leftVelocity = left.body.linearVelocity;
        Vector2 rightVelocity = right.body.linearVelocity;
        leftVelocity.x = leftSpeed;
        rightVelocity.x = rightSpeed;
        left.body.linearVelocity = leftVelocity;
        right.body.linearVelocity = rightVelocity;
    }
}
