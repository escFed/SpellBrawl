using UnityEngine;

/// <summary>Actions used by the reusable combat Canvas for scene-owned managers.</summary>
public sealed class CombatCanvasActions : MonoBehaviour
{
    public void ResumeGame()
    {
        PauseMenu pause = FindAnyObjectByType<PauseMenu>();
        if (pause != null) pause.ResumeGame();
        else Debug.LogError("[CombatCanvasActions] PauseMenu is missing.", this);
    }

    public void ShowSettings()
    {
        PauseMenu pause = FindAnyObjectByType<PauseMenu>();
        if (pause != null) pause.ShowSettings();
        else Debug.LogError("[CombatCanvasActions] PauseMenu is missing.", this);
    }

    public void HideSettings()
    {
        PauseMenu pause = FindAnyObjectByType<PauseMenu>();
        if (pause != null) pause.HideSettings();
        else Debug.LogError("[CombatCanvasActions] PauseMenu is missing.", this);
    }

    public void Rematch()
    {
        if (GameManager.Instance != null) GameManager.Instance.Rematch();
        else Debug.LogError("[CombatCanvasActions] GameManager is missing.", this);
    }

    public void GoToMainMenu()
    {
        if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        else Debug.LogError("[CombatCanvasActions] GameManager is missing.", this);
    }
}
