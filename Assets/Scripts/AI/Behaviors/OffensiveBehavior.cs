using UnityEngine;

public class OffensiveBehavior : AIBehavior
{
    private float nextAttackTime;
    private float nextCardTime;
    private int consecutiveAttacks;
    private float targetTimer;

    public override void Enter(PlayerAI ai)
    {
        consecutiveAttacks = 0;
        targetTimer = 0f;
    }

    public override void UpdateBehavior(PlayerAI ai)
    {
        targetTimer += Time.deltaTime;

        if (targetTimer > 3f && Mathf.Abs(ai.Target.position.y - ai.transform.position.y) > 1.5f)
        {
            ai.ChangeBehavior(ai.patrolBehavior);
            return;
        }

        float distX = ai.Target.position.x - ai.transform.position.x;
        float distY = ai.Target.position.y - ai.transform.position.y;
        float absDistX = Mathf.Abs(distX);
        float dirX = Mathf.Sign(distX);

        if (Time.time >= nextCardTime && absDistX < ai.cardRange)
        {
            if (Random.value < 0.5f) ai.TriggerHand1();
            else ai.TriggerHand2();

            nextCardTime = Time.time + ai.timeBetweenCards;
            targetTimer = 0f;

            if (Random.value > 0.7f)
            {
                ai.ChangeBehavior(ai.defensiveBehavior);
                return;
            }
        }

        if (absDistX > ai.attackRange || Mathf.Abs(distY) > 1.5f)
        {
            if (ai.IsSafeToMove(dirX))
            {
                ai.SetDirection(new Vector2(dirX, 0));
            }
            else
            {
                if (distY > -1f && ai.SelfController.IsGrounded)
                {
                    ai.TriggerJump();
                    ai.SetDirection(new Vector2(dirX, 0));
                }
                else if (distY <= -1f)
                {
                    ai.SetDirection(new Vector2(dirX, 0));
                }
            }

            if (distY > 2f && ai.SelfController.IsGrounded)
            {
                ai.TriggerJump();
            }
        }
        else
        {
            if (absDistX < 0.6f)
            {
                if (Time.time < nextAttackTime)
                {
                    ai.SetDirection(new Vector2(dirX, 0));
                }
                else
                {
                    float randAttack = Random.value;
                    if (randAttack < 0.25f) ai.SetDirection(new Vector2(0, 1));
                    else if (randAttack < 0.5f) ai.SetDirection(new Vector2(0, -1));
                    else if (randAttack < 0.75f) ai.SetDirection(new Vector2(dirX, 0));
                    else ai.SetDirection(Vector2.zero);

                    ai.TriggerAttack();
                    nextAttackTime = Time.time + ai.attackCooldown;
                    consecutiveAttacks++;
                    targetTimer = 0f;
                }
            }
            else
            {
                ai.SetDirection(Vector2.zero);

                if (Time.time >= nextAttackTime)
                {
                    ai.TriggerAttack();
                    nextAttackTime = Time.time + ai.attackCooldown;
                    consecutiveAttacks++;
                    targetTimer = 0f;
                }
            }

            if (consecutiveAttacks >= Random.Range(2, 5))
            {
                ai.ChangeBehavior(ai.defensiveBehavior);
            }
        }
    }

    public override void Exit(PlayerAI ai) { }
}
