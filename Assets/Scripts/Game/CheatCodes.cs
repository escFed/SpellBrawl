using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodes : MonoBehaviour
{
    private InputActionMap cheatActions;
    private InputAction roundReset;
    private InputAction matchRestart;
    private InputAction stocksP1One;
    private InputAction aiFreezeP2;

    private void OnEnable()
    {
        cheatActions = InputSystem.actions?.FindActionMap("Cheats");
        if (cheatActions == null)
        {
            Debug.LogWarning("[CheatCodes] The project-wide Cheats action map is missing.", this);
            return;
        }

        roundReset = cheatActions.FindAction("RoundReset");
        matchRestart = cheatActions.FindAction("MatchRestart");
        stocksP1One = cheatActions.FindAction("StocksP1One");
        aiFreezeP2 = cheatActions.FindAction("AIFreezeP2");
        if (roundReset == null || matchRestart == null || stocksP1One == null || aiFreezeP2 == null)
        {
            Debug.LogWarning("[CheatCodes] One or more cheat actions are missing.", this);
            return;
        }

        roundReset.performed += OnRoundReset;
        matchRestart.performed += OnMatchRestart;
        stocksP1One.performed += OnStocksP1One;
        aiFreezeP2.performed += OnAIFreezeP2;
        cheatActions.Enable();
    }

    private void OnDisable()
    {
        if (roundReset != null) roundReset.performed -= OnRoundReset;
        if (matchRestart != null) matchRestart.performed -= OnMatchRestart;
        if (stocksP1One != null) stocksP1One.performed -= OnStocksP1One;
        if (aiFreezeP2 != null) aiFreezeP2.performed -= OnAIFreezeP2;
        cheatActions?.Disable();
    }

    private void OnRoundReset(InputAction.CallbackContext _) => GameManager.Instance?.DebugResetRound();

    private void OnMatchRestart(InputAction.CallbackContext _) => GameManager.Instance?.Rematch();

    private void OnStocksP1One(InputAction.CallbackContext _)
    {
        CharacterHealth health = RespawnManager.Instance != null && RespawnManager.Instance.p1Instance != null
            ? RespawnManager.Instance.p1Instance.GetComponent<CharacterHealth>()
            : null;
        if (health == null || health.IsDead)
        {
            Debug.LogWarning("[Cheat] P1 must be alive to set stocks. Use F9 to reset the round.", this);
            return;
        }

        health.fallLives = 1;
        health.UpdateUI();
        Debug.Log("[Cheat] P1 stocks set to 1.", this);
    }

    private void OnAIFreezeP2(InputAction.CallbackContext _)
    {
        CharacterCoordinator playerTwo = RespawnManager.Instance != null && RespawnManager.Instance.p2Instance != null
            ? RespawnManager.Instance.p2Instance.GetComponent<CharacterCoordinator>()
            : null;
        CharacterAI ai = playerTwo != null && playerTwo.Mode == PlayerMode.AI
            ? playerTwo.GetComponent<CharacterAI>()
            : null;
        if (ai == null)
        {
            Debug.LogWarning("[Cheat] P2 is not controlled by AI.", this);
            return;
        }

        ai.SetDebugFrozen(!ai.IsDebugFrozen);
        Debug.Log(ai.IsDebugFrozen ? "[Cheat] P2 AI frozen." : "[Cheat] P2 AI resumed.", this);
    }
}
