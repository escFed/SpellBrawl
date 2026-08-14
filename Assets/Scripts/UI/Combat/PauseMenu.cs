using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public static bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        UIFocus.Clear();
        isPaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        bool keyboardPause = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepadPause = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (keyboardPause || gamepadPause)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                HideSettings();
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;
        UIFocus.SelectFirst(pausePanel);
    }

    public void ResumeGame()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        UIFocus.Clear();
    }

    public void ShowSettings()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            if (settingsPanel.TryGetComponent(out SettingsController settingsController))
            {
                settingsController.ShowSettingsHome();
            }

            UIFocus.SelectFirst(settingsPanel);
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        UIFocus.SelectFirst(pausePanel);
    }
}
