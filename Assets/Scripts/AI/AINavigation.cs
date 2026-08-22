using UnityEngine;

[System.Serializable]
public class AINavigation
{
    public LayerMask groundLayer;
    public float lookAheadDistance = 1f;
    public float fallCheckDepth = 5f;
    public float verticalJumpThreshold = 1.5f;
    public float recoveryHeightThreshold = -3f;

    public bool IsNearEdge(PlayerController selfController, Transform selfTransform)
    {
        if (!selfController.IsGrounded)
            return false;

        Vector2 leftCheck = new Vector2(selfTransform.position.x - lookAheadDistance, selfTransform.position.y);
        Vector2 rightCheck = new Vector2(selfTransform.position.x + lookAheadDistance, selfTransform.position.y);

        bool hasLeftGround = Physics2D.Raycast(leftCheck, Vector2.down, fallCheckDepth, groundLayer).collider != null;
        bool hasRightGround = Physics2D.Raycast(rightCheck, Vector2.down, fallCheckDepth, groundLayer).collider != null;

        return !hasLeftGround || !hasRightGround;
    }

    public bool ShouldRecover(Transform selfTransform, CharacterHealth selfHealth, Vector3 targetPosition, bool nearEdge)
    {
        bool tooLow = selfTransform.position.y < targetPosition.y + recoveryHeightThreshold;
        bool highDamageNearEdge = nearEdge && selfHealth != null && selfHealth.currentDamage >= 100f;

        return tooLow || highDamageNearEdge;
    }

    public void ExecuteMove(PlayerController selfController, Transform selfTransform, AIInput input, float directionX, bool forceJump)
    {
        if (Mathf.Abs(directionX) < 0.01f)
        {
            input.SetDirection(Vector2.zero);
            return;
        }

        bool shouldJump;
        bool safe = IsSafeToMove(selfController, selfTransform, directionX, out shouldJump);

        if (!safe)
        {
            input.SetDirection(Vector2.zero);
            return;
        }

        input.SetDirection(new Vector2(directionX, 0f));

        if (forceJump || shouldJump)
            input.PressJump();
    }

    private bool IsSafeToMove(PlayerController selfController, Transform selfTransform, float directionX, out bool shouldJump)
    {
        shouldJump = false;

        if (!selfController.IsGrounded)
            return true;

        Vector2 edgeCheck = new Vector2(selfTransform.position.x + directionX * lookAheadDistance, selfTransform.position.y);

        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheck, Vector2.down, fallCheckDepth, groundLayer);

        if (edgeHit.collider == null)
        {
            shouldJump = true;
            return true;
        }

        return true;
    }
}

