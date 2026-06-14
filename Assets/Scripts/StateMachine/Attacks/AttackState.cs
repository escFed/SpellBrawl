using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class AttackState : PlayerState
{
    protected AttackStats stats;
    private float timer;
    private bool hitboxOpen;
    private bool hitboxClose;
    private PlayerController controller;
    private float lockedX = 0f;
    public AttackState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm)
    {
        stats = attackStats;
        controller = character;
    }

    public override void Enter()
    {
        timer = 0;
        hitboxOpen = false;
        hitboxClose = false;
        ReadyHitbox();
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stats.startup && !hitboxOpen)
        {
            OpenHitbox();
            hitboxOpen = true;
        }

        if (timer >= stats.startup + stats.active && !hitboxClose)
        {
            CloseHitbox();
            hitboxClose = true;
        }

        if (timer >= stats.startup + stats.active + stats.recovery)
        {
            stateMachine.ChangeState(StateCharacter.Idle);
        }
    }

    protected abstract void ReadyHitbox();
    protected abstract void OpenHitbox();
    protected abstract void CloseHitbox();
}
