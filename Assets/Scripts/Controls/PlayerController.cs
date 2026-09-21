using UnityEngine.InputSystem;

public class PlayerController
{
    private CharacterBrain brain;
    private CharacterAI ai;
    private PlayerInput playerInput;
    private bool usesExternalProvider;

    public PlayerMode Mode { get; private set; }
    public IInputProvider ActiveInput { get; private set; }
    public bool InputEnabled { get; private set; }

    public PlayerController(CharacterBrain characterBrain, CharacterAI characterAI, bool inputEnabled)
    {
        brain = characterBrain;
        this.ai = characterAI;
        playerInput = characterBrain != null? characterBrain.GetComponent<PlayerInput>() : null;
        InputEnabled = inputEnabled;
    }

    public bool Configure(PlayerMode mode)
    {
        IInputProvider nextInput = mode == PlayerMode.Player ? brain : ai;

        if (nextInput == null)
            return false;

        ClearAllInputs();
        usesExternalProvider = false;
        Mode = mode;
        ActiveInput = nextInput;
        ApplyInputEnabled();
        return true;
    }

    public bool ConfigureInputProvider(IInputProvider inputProvider)
    {
        if (inputProvider == null)
            return false;

        ClearAllInputs();
        usesExternalProvider = true;
        ActiveInput = inputProvider;
        ApplyInputEnabled();
        return true;
    }

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;
        ApplyInputEnabled();
    }

    private void ApplyInputEnabled()
    {
        bool playerActive = InputEnabled && !usesExternalProvider && Mode == PlayerMode.Player;
        bool aiActive = InputEnabled && !usesExternalProvider && Mode == PlayerMode.AI;

        if (brain != null)
            brain.enabled = playerActive;

        if (playerInput != null)
            playerInput.enabled = playerActive;
        if (ai != null)
            ai.enabled = aiActive;

        if (!InputEnabled)
            ActiveInput?.ClearAllInputs();
    }

    public void ClearAllInputs()
    {
        brain?.ClearAllInputs();
        ai?.ClearAllInputs();

        if (usesExternalProvider)
            ActiveInput?.ClearAllInputs();
    }
}
