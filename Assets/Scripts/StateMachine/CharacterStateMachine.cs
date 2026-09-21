using System;

public sealed class CharacterStateMachine
{
    private const int MaximumTransitionsPerRequest = 8;

    private bool isTransitioning;
    private ICharacterState pendingState;

    public ICharacterState CurrentState { get; private set; }

    public bool Is(ICharacterState state) => state != null && ReferenceEquals(CurrentState, state);

    public void ChangeState(ICharacterState nextState)
    {
        if (nextState == null)
            throw new ArgumentNullException(nameof(nextState));

        if (isTransitioning)
        {
            // Finish the current Exit/Enter pair before starting another transition.
            pendingState = nextState;
            return;
        }

        if (Is(nextState))
            return;

        ICharacterState requestedState = nextState;
        for (int count = 0; count < MaximumTransitionsPerRequest; count++)
        {
            TransitionImmediately(requestedState);

            if (pendingState == null)
                return;

            requestedState = pendingState;
            pendingState = null;
            if (Is(requestedState))
                return;
        }

        pendingState = null;
        throw new InvalidOperationException("Too many chained character state transitions.");
    }

    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();

    private void TransitionImmediately(ICharacterState nextState)
    {
        ICharacterState previousState = CurrentState;
        isTransitioning = true;
        try
        {
            previousState?.Exit();
            CurrentState = nextState;
            nextState.Enter();
        }
        finally
        {
            isTransitioning = false;
        }
    }
}
