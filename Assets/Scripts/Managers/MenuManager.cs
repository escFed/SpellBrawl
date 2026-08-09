using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject howToPlayPanel;
    [SerializeField]
    private GameObject controlsPanel;
    [SerializeField]
    private GameObject characterSelectPanel;
    [SerializeField]
    private GameObject cardsSelectPanel;
    [SerializeField]
    private GameObject settingsPanel;


    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        CloseSettings();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void ShowHowToPlay()
    {

        CloseSettings();


        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        
    }

    public void CharacterSelect()
    {
        CloseSettings();
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = false;

        mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
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
    }

    public void GoToTrainingRoom()
    {
        SceneManager.LoadScene("TrainingRoom");
    }

    public void ShowCardsSelect()
    {
        CloseSettings();
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(true);
    }

    public void BackToCharacterSelect()
    {
        CloseSettings();
        cardsSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void GoToStage1()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowSettings()
    {
        if(mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        ShowMainMenu();
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
