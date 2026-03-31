public abstract class PlayerState : IState
{
    protected PlayerController player;
    protected StateMachine stateMachine;

    protected PlayerState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
