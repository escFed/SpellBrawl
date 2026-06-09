using UnityEngine;

public class AIExecutor
{
    private int consecutiveAttacks;
    private float attackCooldownTimer;
    private float cardCooldownTimer;
    private AIDecision lastLoggedDecision = (AIDecision)(-1);
    private string lastLoggedAttack = "";

    public float AttackCooldownTimer => attackCooldownTimer;
    public float CardCooldownTimer => cardCooldownTimer;

    public void Tick(float deltaTime)
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= deltaTime;

        if (cardCooldownTimer > 0f)
            cardCooldownTimer -= deltaTime;
    }

    public void Execute(AIDecision decision, AIInput input, PlayerController selfController, Transform selfTransform, AINavigation navigation, Vector3 perceivedTargetPosition, float attackRange, int selectedCardIndex, int maxConsecutiveAttacks, float attackCooldownDuration, float cardCooldownDuration)
    {
        input.ClearAll();

        float deltaX = perceivedTargetPosition.x - selfTransform.position.x;
        float dirX = Mathf.Abs(deltaX) < 0.01f ? GetFacingDirection(selfTransform): Mathf.Sign(deltaX);

        LogDecisionChange(decision);
        UpdateAttackLimit(decision, maxConsecutiveAttacks, attackCooldownDuration);

        switch (decision)
        {
            case AIDecision.Idle:
                input.SetDirection(Vector2.zero);
                break;

            case AIDecision.Chase:
                navigation.ExecuteMove(selfController, selfTransform, input, dirX, false);
                break;

            case AIDecision.Flee:
            case AIDecision.Reposition:
                navigation.ExecuteMove(selfController, selfTransform, input, -dirX, false);
                break;

            case AIDecision.Recover:
                navigation.ExecuteMove(selfController, selfTransform, input, dirX, true);
                break;

            case AIDecision.Jump:
                input.SetDirection(new Vector2(dirX * 0.5f, 0f));
                input.PressJump();
                break;

            case AIDecision.Attack:
                ExecuteAttack(input, selfTransform, perceivedTargetPosition, dirX, attackRange);
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
                input.SetDirection(new Vector2(dirX, 0f));
                input.PressCardButton(selectedCardIndex);
                cardCooldownTimer = cardCooldownDuration;
                break;
        }
    }

    private void ExecuteAttack(AIInput input, Transform selfTransform, Vector3 perceivedTargetPosition, float dirX, float attackRange)
    {
        float distY = perceivedTargetPosition.y - selfTransform.position.y;
        float distXAbs = Mathf.Abs(perceivedTargetPosition.x - selfTransform.position.x);

        Vector2 attackDirection;
        string attackName;

        if (distY > 0.5f)
        {
            attackDirection = new Vector2(0f, 1f);
            attackName = "Up Tilt";
        }
        else if (distY < -0.2f)
        {
            attackDirection = new Vector2(0f, -1f);
            attackName = "Down Tilt";
        }
        else if (distXAbs > attackRange * 0.5f)
        {
            attackDirection = new Vector2(dirX, 0f);
            attackName = "Forward Tilt";
        }
        else if (Random.value > 0.5f)
        {
            attackDirection = Vector2.zero;
            attackName = "Jab";
        }
        else
        {
            attackDirection = new Vector2(dirX, 0f);
            attackName = "Forward Tilt";
        }

        if (attackName != lastLoggedAttack)
        {
            Debug.Log($"<color=red>[IA Combat]</color> Ejecutando: <b>{attackName}</b>");
            lastLoggedAttack = attackName;
        }

        input.SetDirection(attackDirection);
        input.PressAttack();
    }

    private void UpdateAttackLimit(AIDecision decision, int maxConsecutiveAttacks, float attackCooldownDuration)
    {
        if (decision == AIDecision.Attack)
        {
            consecutiveAttacks++;

            if (consecutiveAttacks >= maxConsecutiveAttacks)
            {
                attackCooldownTimer = attackCooldownDuration;
                consecutiveAttacks = 0;
                Debug.Log("<color=yellow>[IA Brain]</color> Limite de ataques. Necesito retroceder.");
            }

            return;
        }

        if (attackCooldownTimer <= 0f)
            consecutiveAttacks = 0;
    }

    private void LogDecisionChange(AIDecision decision)
    {
        if (decision == lastLoggedDecision)
            return;

        Debug.Log($"<color=orange>[IA Brain]</color> Cambio de decision a: <b>{decision}</b>");
        lastLoggedDecision = decision;
    }

    private float GetFacingDirection(Transform selfTransform)
    {
        return selfTransform.localScale.x >= 0f ? 1f : -1f;
    }
}

