using UnityEngine;

public class DefensiveBehavior : AIBehavior
{
    private float defensiveTimer;

    public override void Enter(PlayerAI ai)
    {
        defensiveTimer = Random.Range(1.0f, 2.5f);
    }

    public override void UpdateBehavior(PlayerAI ai)
    {
        defensiveTimer -= ai.reactionTime;

        if (defensiveTimer <= 0)
        {
            ai.ChangeBehavior(ai.offensiveBehavior);
            return;
        }

        float distX = ai.Target.position.x - ai.transform.position.x;
        float absDistX = Mathf.Abs(distX);
        float dirX = -Mathf.Sign(distX);

        if (absDistX < ai.cardRange)
        {
            if (ai.IsSafeToMove(dirX))
            {
                ai.SetDirection(new Vector2(dirX, 0));
            }
            else
            {
                if (absDistX < ai.attackRange)
                {
                    ai.SetDirection(Vector2.zero);
                    ai.ChangeBehavior(ai.offensiveBehavior);
                }
                else
                {
                    ai.SetDirection(new Vector2(-dirX, 0));
                }
            }
        }
        else
        {
            ai.SetDirection(new Vector2(-dirX, 0));
        }
    }

    public override void Exit(PlayerAI ai) { }
}
