using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AINavigation
{
    private const float LandingMargin = 0.15f;
    private const float GroundTolerance = 0.35f;
    private const float JumpReachFactor = 0.72f;
    private const float LandingPause = 0.55f;

    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 5f;
    public float verticalJumpThreshold = 1.5f;
    public float recoveryHeightThreshold = -3f;

    private List<Collider2D> platforms = new List<Collider2D>();
    private int[] parents;
    private int[] queue;
    private Collider2D plannedPlatform;
    private float takeoffX;
    private float landingX;
    private bool jumpCommitted;
    private bool leftGroundAfterJump;
    private float jumpDeadline;
    private float nextJumpAllowedAt;
    private float lastRecoveryJumpAt = -10f;
    private CharacterCoordinator self;
    private Transform selfTransform;
    private Collider2D bodyCollider;
    private Rigidbody2D body;
    private float targetDirection;

    public float VerticalJumpThreshold => verticalJumpThreshold;
    public bool HasCommittedJump => jumpCommitted;
    public bool HasPlannedJump => plannedPlatform != null && self != null && self.CanJump && !jumpCommitted;
    public bool CanShortHopSafely => HasPlannedJump &&
        CanReach(self, plannedPlatform, true, out _, out _);
    public bool CanChaseSafely => jumpCommitted || plannedPlatform != null ||
        CanMoveDirection(targetDirection);
    public bool CanFleeSafely => CanMoveDirection(-targetDirection);
    public bool CanDashSafely => self != null && self.IsGrounded &&
        IsGroundPathSafe(targetDirection, self.stats.dashSpeed * self.stats.dashDuration);
    public bool CanEvadeSafely => self != null && self.IsGrounded &&
        IsGroundPathSafe(-targetDirection, self.stats.dodgeSpeed * 0.18f);
    public bool CanFastFallSafely => selfTransform != null &&
        HasGroundBelow(selfTransform.position.x, FeetY, fallCheckDepth);

    private float FeetY => bodyCollider != null ? bodyCollider.bounds.min.y : selfTransform.position.y;
    private float HalfWidth => bodyCollider != null ? bodyCollider.bounds.extents.x : 0.3f;

    public void Configure(
        LayerMask groundLayer,
        float lookAheadDistance,
        float fallCheckDepth,
        float verticalJumpThreshold,
        float recoveryHeightThreshold)
    {
        this.groundLayer = groundLayer;
        this.lookAheadDistance = lookAheadDistance;
        this.fallCheckDepth = fallCheckDepth;
        this.verticalJumpThreshold = verticalJumpThreshold;
        this.recoveryHeightThreshold = recoveryHeightThreshold;
        platforms.Clear();
        ClearJumpPlan();
    }

    public void Reset()
    {
        ClearJumpPlan();
        nextJumpAllowedAt = 0f;
        lastRecoveryJumpAt = -10f;
    }

    public void Refresh(CharacterCoordinator selfController, Transform characterTransform, Vector3 perceivedTargetPosition)
    {
        self = selfController;
        selfTransform = characterTransform;
        bodyCollider ??= characterTransform.GetComponent<Collider2D>();
        body ??= characterTransform.GetComponent<Rigidbody2D>();
        float deltaX = perceivedTargetPosition.x - characterTransform.position.x;
        targetDirection = Mathf.Abs(deltaX) < 0.01f ? 0f : Mathf.Sign(deltaX);

        UpdateCommittedJump();
        if (jumpCommitted)
            return;

        plannedPlatform = null;
        if (Time.time < nextJumpAllowedAt || !selfController.IsGrounded || !selfController.CanJump)
            return;

        Collider2D support = GetSupport();
        if (support == null)
            return;

        if (platforms.Count == 0)
            CollectPlatforms();

        Collider2D goal = GetGroundAt(perceivedTargetPosition.x,
            perceivedTargetPosition.y + 0.3f, fallCheckDepth + 0.3f);
        if (goal == null || goal == support)
            return;

        int startIndex = platforms.IndexOf(support);
        int goalIndex = platforms.IndexOf(goal);
        if (startIndex < 0 || goalIndex < 0)
            return;

        int nextIndex = FindNextPlatform(startIndex, goalIndex);
        if (nextIndex < 0)
            return;

        Collider2D next = platforms[nextIndex];
        if (CanReach(selfController, next, false, out takeoffX, out landingX))
            plannedPlatform = next;
    }

    public void GuardInput(AIInput input, AIDecision decision)
    {
        if (self == null || selfTransform == null)
            return;

        UpdateCommittedJump();
        if (jumpCommitted)
        {
            SteerToward(input, landingX);
            return;
        }

        if (!self.IsGrounded ||
            (decision != AIDecision.Chase && decision != AIDecision.Flee &&
             decision != AIDecision.Reposition && decision != AIDecision.Jump &&
             decision != AIDecision.ShortHop))
            return;

        float direction = input.CurrentDirection.x;
        if (Mathf.Abs(direction) < 0.01f)
            return;

        bool safe = plannedPlatform != null && Mathf.Sign(direction) == Mathf.Sign(takeoffX - selfTransform.position.x)
            ? IsGroundPathSafe(direction, Mathf.Min(Mathf.Abs(takeoffX - selfTransform.position.x), lookAheadDistance))
            : CanMoveDirection(direction);
        if (!safe)
            input.SetDirection(Vector2.zero);
    }

    public bool IsNearEdge(CharacterCoordinator selfController, Transform characterTransform)
    {
        if (!selfController.IsGrounded)
            return false;

        return !HasGroundBelow(characterTransform.position.x - lookAheadDistance, FeetY, GroundTolerance) ||
            !HasGroundBelow(characterTransform.position.x + lookAheadDistance, FeetY, GroundTolerance);
    }

    public bool ShouldRecover(CharacterCoordinator selfController, Transform characterTransform)
    {
        if (selfController == null || selfController.IsGrounded)
            return false;

        float predictedX = characterTransform.position.x + (body != null ? body.linearVelocity.x * 0.25f : 0f);
        return !HasGroundBelow(predictedX, FeetY, fallCheckDepth);
    }

    public void ExecuteMove(
        CharacterCoordinator selfController,
        Transform characterTransform,
        AIInput input,
        float directionX,
        bool forceJump,
        Vector3 perceivedTargetPosition)
    {
        if (forceJump)
        {
            ExecuteRecovery(selfController, input);
            return;
        }

        if (jumpCommitted)
        {
            SteerToward(input, landingX);
            return;
        }

        if (plannedPlatform != null && Mathf.Sign(directionX) == targetDirection)
        {
            ExecutePlannedJump(selfController, input, false);
            return;
        }

        input.SetDirection(CanMoveDirection(directionX)
            ? new Vector2(Mathf.Sign(directionX), 0f)
            : Vector2.zero);
    }

    public void ExecutePlannedJump(CharacterCoordinator selfController, AIInput input, bool shortHop)
    {
        if (plannedPlatform == null || !selfController.CanJump ||
            (shortHop && !CanShortHopSafely))
        {
            input.SetDirection(Vector2.zero);
            return;
        }

        float distanceToTakeoff = takeoffX - selfTransform.position.x;
        if (Mathf.Abs(distanceToTakeoff) > 0.22f)
        {
            float direction = Mathf.Sign(distanceToTakeoff);
            input.SetDirection(IsGroundPathSafe(direction, Mathf.Min(Mathf.Abs(distanceToTakeoff), lookAheadDistance))
                ? new Vector2(direction, 0f) : Vector2.zero);
            return;
        }

        jumpCommitted = true;
        leftGroundAfterJump = false;
        jumpDeadline = Time.time + 3f;
        SteerToward(input, landingX);
        input.PressJump();
    }

    private void ExecuteRecovery(CharacterCoordinator selfController, AIInput input)
    {
        if (platforms.Count == 0)
            CollectPlatforms();

        Collider2D nearest = null;
        float bestDistance = float.PositiveInfinity;
        for (int i = 0; i < platforms.Count; i++)
        {
            Collider2D platform = platforms[i];
            if (platform == null || !platform.enabled || !platform.gameObject.activeInHierarchy)
                continue;

            float candidateX = Mathf.Clamp(selfTransform.position.x,
                platform.bounds.min.x + LandingMargin, platform.bounds.max.x - LandingMargin);
            float distance = Mathf.Abs(candidateX - selfTransform.position.x) +
                Mathf.Max(0f, platform.bounds.max.y - FeetY) * 0.25f;
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            nearest = platform;
        }

        if (nearest == null)
        {
            input.SetDirection(Vector2.zero);
            return;
        }

        float destinationX = Mathf.Clamp(selfTransform.position.x,
            nearest.bounds.min.x + LandingMargin, nearest.bounds.max.x - LandingMargin);
        SteerToward(input, destinationX);
        if (selfController.CanJump && Time.time - lastRecoveryJumpAt > 0.45f &&
            FeetY < nearest.bounds.max.y + 0.4f)
        {
            input.PressJump();
            lastRecoveryJumpAt = Time.time;
        }
    }

    private void UpdateCommittedJump()
    {
        if (!jumpCommitted || self == null)
            return;

        if (!self.IsGrounded)
            leftGroundAfterJump = true;

        if ((leftGroundAfterJump && self.IsGrounded) || Time.time >= jumpDeadline)
        {
            ClearJumpPlan();
            nextJumpAllowedAt = Time.time + LandingPause;
        }
    }

    private void ClearJumpPlan()
    {
        plannedPlatform = null;
        jumpCommitted = false;
        leftGroundAfterJump = false;
    }

    private bool CanMoveDirection(float direction)
    {
        if (self == null || selfTransform == null || Mathf.Abs(direction) < 0.01f)
            return false;
        if (!self.IsGrounded)
            return HasGroundBelow(selfTransform.position.x + Mathf.Sign(direction) * lookAheadDistance,
                FeetY, fallCheckDepth);

        return IsGroundPathSafe(direction, Mathf.Max(lookAheadDistance,
            HalfWidth + self.stats.moveSpeed * Time.fixedDeltaTime * 2f));
    }

    private bool IsGroundPathSafe(float direction, float distance)
    {
        if (selfTransform == null || Mathf.Abs(direction) < 0.01f)
            return false;

        float step = Mathf.Max(0.2f, HalfWidth * 0.8f);
        int samples = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(distance) / step));
        for (int i = 1; i <= samples; i++)
        {
            float x = selfTransform.position.x + Mathf.Sign(direction) * Mathf.Abs(distance) * i / samples;
            if (!HasGroundBelow(x, FeetY, GroundTolerance))
                return false;
        }
        return true;
    }

    private bool HasGroundBelow(float x, float feetY, float depth)
    {
        return GetGroundAt(x, feetY + 0.2f, depth + 0.2f) != null;
    }

    private Collider2D GetGroundAt(float x, float originY, float depth)
    {
        if (groundLayer.value == 0)
            return null;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, originY), Vector2.down, depth, groundLayer);
        return hit.collider != null && !hit.collider.isTrigger ? hit.collider : null;
    }

    private Collider2D GetSupport()
    {
        return GetGroundAt(selfTransform.position.x, FeetY + 0.2f, 0.5f);
    }

    private void CollectPlatforms()
    {
        platforms.Clear();
        Collider2D[] colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider.enabled && !collider.isTrigger &&
                (groundLayer.value & (1 << collider.gameObject.layer)) != 0)
                platforms.Add(collider);
        }
        parents = new int[platforms.Count];
        queue = new int[platforms.Count];
    }

    private int FindNextPlatform(int start, int goal)
    {
        for (int i = 0; i < parents.Length; i++)
            parents[i] = -1;

        int head = 0;
        int tail = 0;
        queue[tail++] = start;
        parents[start] = start;

        while (head < tail && parents[goal] < 0)
        {
            int from = queue[head++];
            for (int to = 0; to < platforms.Count; to++)
            {
                if (parents[to] >= 0 || from == to ||
                    !CanReach(self, platforms[from], platforms[to], false, out _, out _))
                    continue;

                parents[to] = from;
                queue[tail++] = to;
            }
        }

        if (parents[goal] < 0)
            return -1;

        int next = goal;
        while (parents[next] != start)
            next = parents[next];
        return next;
    }

    private bool CanReach(CharacterCoordinator controller, Collider2D destination,
        bool shortHop, out float departureX, out float destinationX)
    {
        return CanReach(controller, GetSupport(), destination, shortHop, out departureX, out destinationX);
    }

    private bool CanReach(CharacterCoordinator controller, Collider2D sourceCollider,
        Collider2D destinationCollider, bool shortHop, out float departureX, out float destinationX)
    {
        departureX = destinationX = 0f;
        if (controller == null || sourceCollider == null || destinationCollider == null ||
            sourceCollider == destinationCollider)
            return false;

        if (body == null || controller.stats == null)
            return false;

        float margin = HalfWidth + LandingMargin;
        Bounds source = sourceCollider.bounds;
        Bounds destination = destinationCollider.bounds;
        if (source.size.x < margin * 2f || destination.size.x < margin * 2f)
            return false;

        departureX = Mathf.Clamp(destination.center.x, source.min.x + margin, source.max.x - margin);
        destinationX = Mathf.Clamp(departureX, destination.min.x + margin, destination.max.x - margin);
        float multiplier = Mathf.Max(0.01f, controller.stats.jumpSpeedMultiplier);
        float verticalSpeed = controller.stats.jumpForce * multiplier *
            (shortHop ? Mathf.Clamp01(controller.stats.shortHopVelocityMultiplier) : 1f);
        float gravity = Mathf.Abs(Physics2D.gravity.y * body.gravityScale) * multiplier * multiplier;
        if (gravity < 0.01f || verticalSpeed <= 0f)
            return false;

        float heightDifference = destination.max.y - source.max.y;
        float discriminant = verticalSpeed * verticalSpeed - 2f * gravity * heightDifference;
        if (discriminant <= 0f)
            return false;

        float flightTime = (verticalSpeed + Mathf.Sqrt(discriminant)) / gravity;
        float maximumTravel = controller.stats.moveSpeed * flightTime * JumpReachFactor;
        if (destination.max.y < source.max.y - 0.1f &&
            destinationX > source.min.x - HalfWidth &&
            destinationX < source.max.x + HalfWidth)
            return false;

        float apexHeight = verticalSpeed * verticalSpeed / (2f * gravity);
        if (!HasJumpClearance(departureX, source.max.y, apexHeight))
            return false;

        return Mathf.Abs(destinationX - departureX) <= maximumTravel &&
            GetGroundAt(destinationX, destination.max.y + 0.2f, 0.4f) == destinationCollider;
    }

    private bool HasJumpClearance(float departureX, float platformTop, float apexHeight)
    {
        float headY = platformTop + (bodyCollider != null ? bodyCollider.bounds.size.y : 0.8f) + 0.05f;
        float width = HalfWidth * 0.8f;
        for (int i = -1; i <= 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(departureX + i * width, headY),
                Vector2.up,
                apexHeight,
                groundLayer);
            if (hit.collider != null && !hit.collider.isTrigger)
                return false;
        }
        return true;
    }

    private void SteerToward(AIInput input, float destinationX)
    {
        float distance = destinationX - selfTransform.position.x;
        input.SetDirection(Mathf.Abs(distance) < 0.08f
            ? Vector2.zero : new Vector2(Mathf.Sign(distance), 0f));
    }
}
