using UnityEngine;

public class PatrolBehavior : AIBehavior
{
    private float patrolDirection;
    private float changeDirectionTimer;

    public override void Enter(PlayerAI ai)
    {
        patrolDirection = Random.value > 0.5f ? 1f : -1f;
        changeDirectionTimer = Random.Range(1.5f, 3f);
    }

    public override void UpdateBehavior(PlayerAI ai)
    {
        changeDirectionTimer -= Time.deltaTime;

        float distX = ai.Target.position.x - ai.transform.position.x;
        float distY = ai.Target.position.y - ai.transform.position.y;

        if (Mathf.Abs(distY) < 1.5f || Mathf.Abs(distX) < ai.attackRange)
        {
            ai.ChangeBehavior(ai.offensiveBehavior);
            return;
        }

        if (changeDirectionTimer <= 0)
        {
            patrolDirection *= -1f;
            changeDirectionTimer = Random.Range(1.5f, 3f);
        }

        if (ai.IsSafeToMove(patrolDirection))
        {
            ai.SetDirection(new Vector2(patrolDirection, 0));

            if (distY > 1.5f && ai.SelfController.IsGrounded && Random.value < 0.02f)
            {
                ai.TriggerJump();
            }
        }
        else
        {
            if (ai.SelfController.IsGrounded)
            {
                if (Random.value > 0.5f)
                {
                    ai.TriggerJump();
                    ai.SetDirection(new Vector2(patrolDirection, 0));
                }
                else
                {
                    patrolDirection *= -1f;
                    ai.SetDirection(new Vector2(patrolDirection, 0));
                }
            }
        }
    }

    public override void Exit(PlayerAI ai) { }
}
