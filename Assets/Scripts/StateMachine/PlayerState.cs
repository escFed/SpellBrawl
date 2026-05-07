public abstract class PlayerState : IState
{
    protected PlayerController character;
    protected StateMachine stateMachine;

    protected PlayerState(PlayerController character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
