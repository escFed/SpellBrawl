using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject controlsPanel;
    public GameObject characterSelectPanel;
    public GameObject cardsSelectPanel;
    public GameObject settingsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        CloseSettings();
        mainMenuPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        Focus(mainMenuPanel);
    }

    public void ShowHowToPlay()
    {
        CloseSettings();
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        Focus(howToPlayPanel);
    }

    public void CharacterSelect()
    {
        CloseSettings();
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = false;

        mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        Focus(characterSelectPanel);
    }

    public void TrainingRoom()
    {
        CloseSettings();
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = true;

        mainMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        Focus(characterSelectPanel);
    }

    public void GoToTrainingRoom()
    {
        SceneManager.LoadScene("TrainingRoom");
    }

    public void ShowCardsSelect()
    {
        CloseSettings();
        characterSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        cardsSelectPanel.SetActive(true);
        Focus(cardsSelectPanel);
    }

    public void BackToCharacterSelect()
    {
        CloseSettings();
        cardsSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        Focus(characterSelectPanel);
    }

    public void GoToStage1()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        Focus(controlsPanel);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        Focus(howToPlayPanel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            if (settingsPanel.TryGetComponent(out SettingsController settingsController))
            {
                settingsController.ShowSettingsHome();
            }

            Focus(settingsPanel);
        }
    }

    public void HideSettings()
    {
        ShowMainMenu();
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Focus(GameObject panel)
    {
        if (panel != null)
            StartCoroutine(UIFocus.SelectFirstNextFrame(panel));
    }

}
