using UnityEngine;

public class AIExecutor
{
    private float jumpReleaseAt = -1f;
    private float heavyReleaseAt = -1f;
    private float shieldReleaseAt = -1f;

    public void Tick(AIInput input, float currentTime)
    {
        if (jumpReleaseAt >= 0f && currentTime >= jumpReleaseAt)
        {
            input.ReleaseJump();
            jumpReleaseAt = -1f;
        }

        if (heavyReleaseAt >= 0f && currentTime >= heavyReleaseAt)
        {
            input.SetHeavyAttackHeld(false);
            heavyReleaseAt = -1f;
        }

        if (shieldReleaseAt >= 0f && currentTime >= shieldReleaseAt)
        {
            input.SetShieldHeld(false);
            shieldReleaseAt = -1f;
        }
    }

    public void Reset(AIInput input)
    {
        jumpReleaseAt = -1f;
        heavyReleaseAt = -1f;
        shieldReleaseAt = -1f;
        input.ClearAllInputs();
    }

    public void Execute(AIActionScore action, AIInput input, CharacterCoordinator selfController, Transform selfTransform, AINavigation navigation, Vector3 perceivedTargetPosition, float attackRange, AIProfile profile, float currentTime)
    {
        Reset(input);

        float deltaX = perceivedTargetPosition.x - selfTransform.position.x;
        float directionX = Mathf.Abs(deltaX) < 0.01f ? GetFacingDirection(selfTransform): Mathf.Sign(deltaX);

        switch (action.Decision)
        {
            case AIDecision.Idle:
                break;

            case AIDecision.Chase:
                navigation.ExecuteMove(selfController, selfTransform, input, directionX, false, perceivedTargetPosition);
                break;

            case AIDecision.Flee:
            case AIDecision.Reposition:
                navigation.ExecuteMove(selfController, selfTransform, input, -directionX, false, perceivedTargetPosition);
                break;

            case AIDecision.Recover:
                navigation.ExecuteMove(selfController, selfTransform, input, directionX, true, perceivedTargetPosition);
                break;

            case AIDecision.Jump:
                navigation.ExecutePlannedJump(selfController, input, false);
                break;

            case AIDecision.ShortHop:
                navigation.ExecutePlannedJump(selfController, input, true);
                if (input.HasBufferedJump)
                    jumpReleaseAt = currentTime + profile.shortHopHoldTime;
                break;

            case AIDecision.Crouch:
            case AIDecision.FastFall:
                input.SetDirection(Vector2.down);
                break;

            case AIDecision.Attack:
                ExecuteAttack(input, selfTransform, perceivedTargetPosition, directionX, attackRange);
                break;

            case AIDecision.Dash:
                input.SetDirection(new Vector2(directionX, 0f));
                input.PressDash();
                break;

            case AIDecision.DashAttack:
                input.SetDirection(new Vector2(directionX, 0f));
                input.PressDash();
                input.PressAttack();
                break;

            case AIDecision.DashGrab:
                input.SetDirection(new Vector2(directionX, 0f));
                input.PressDash();
                input.PressGrab();
                break;

            case AIDecision.HeavyAttack:
                SetAttackDirection(input, selfTransform, perceivedTargetPosition, directionX, attackRange);
                input.SetHeavyAttackHeld(true);
                heavyReleaseAt = currentTime + profile.heavyChargeTime;
                break;

            case AIDecision.Grab:
                input.SetDirection(new Vector2(directionX, 0f));
                input.PressGrab();
                break;

            case AIDecision.Pummel:
                input.SetDirection(Vector2.zero);
                input.PressAttack();
                break;

            case AIDecision.Throw:
                input.SetDirection(ResolveThrowInput(selfController, selfTransform, navigation, perceivedTargetPosition, directionX));
                input.PressGrab();
                break;

            case AIDecision.Shield:
                input.SetShieldHeld(true);
                shieldReleaseAt = currentTime + profile.shieldHoldTime;
                break;

            case AIDecision.Evade:
                input.SetDirection(new Vector2(-directionX, selfController != null && selfController.IsGrounded ? 0f : 0.35f));
                input.PressEvade();
                break;

            case AIDecision.Parry:
                input.PressParry();
                break;

            case AIDecision.DrawCards:
                input.PressDrawCards();
                break;

            case AIDecision.UseOffensiveCard:
            case AIDecision.UseDefensiveCard:
            case AIDecision.UseUtilityCard:
            case AIDecision.UseBoostCard:
                input.SetDirection(new Vector2(directionX, 0f));
                input.PressCardButton(action.CardIndex);
                break;
        }
    }

    private static void ExecuteAttack(AIInput input, Transform selfTransform, Vector3 perceivedTargetPosition, float directionX, float attackRange)
    {
        SetAttackDirection(input, selfTransform, perceivedTargetPosition, directionX, attackRange);
        input.PressAttack();
    }

    private static void SetAttackDirection(AIInput input, Transform selfTransform, Vector3 perceivedTargetPosition, float directionX, float attackRange)
    {
        float distanceY = perceivedTargetPosition.y - selfTransform.position.y;
        float absoluteDistanceX = Mathf.Abs(perceivedTargetPosition.x - selfTransform.position.x);

        if (distanceY > 0.5f)
            input.SetDirection(Vector2.up);
        else if (distanceY < -0.2f)
            input.SetDirection(Vector2.down);
        else if (absoluteDistanceX > attackRange * 0.5f)
            input.SetDirection(new Vector2(directionX, 0f));
        else
            input.SetDirection(Vector2.zero);
    }

    private static Vector2 ResolveThrowInput(CharacterCoordinator selfController, Transform selfTransform, AINavigation navigation, Vector3 perceivedTargetPosition, float directionX)
    {
        float distanceY = perceivedTargetPosition.y - selfTransform.position.y;
        if (distanceY > 0.75f)
            return Vector2.up;
        if (distanceY < -0.5f)
            return Vector2.down;

        bool shouldBackThrow = selfController != null && ((selfController.Health != null && selfController.Health.currentDamage >= 85f) || navigation.IsNearEdge(selfController, selfTransform));
        if (shouldBackThrow)
            return new Vector2(-GetFacingDirection(selfTransform), 0f);

        float facingDirection = GetFacingDirection(selfTransform);
        return new Vector2(Mathf.Sign(directionX) == Mathf.Sign(facingDirection)? facingDirection: directionX, 0f);
    }

    private static float GetFacingDirection(Transform selfTransform)
    {
        return selfTransform.localScale.x >= 0f ? 1f : -1f;
    }
}
