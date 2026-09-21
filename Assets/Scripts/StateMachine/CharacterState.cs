public abstract class CharacterState : ICharacterState
{
    protected readonly CharacterCoordinator character;
    protected readonly CharacterStateMachine stateMachine;

    protected CharacterState(CharacterCoordinator character, CharacterStateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
