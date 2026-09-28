using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameModePanel;
    public GameObject howToPlayPanel;
    public GameObject controlsPanel;
    public GameObject characterSelectPanel;
    public GameObject cardsSelectPanel;
    public GameObject mapSelectPanel;
    public GameObject settingsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        CloseSettings();
        mainMenuPanel.SetActive(true);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        Focus(mainMenuPanel);
    }

    public void ShowHowToPlay()
    {
        CloseSettings();
        mainMenuPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        Focus(howToPlayPanel);
    }

    public void ShowGameModeSelect()
    {
        CloseSettings();
        mainMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(true);
        Focus(gameModePanel);
    }

    public void SelectPlayerVsPlayer()
    {
        SelectMatchMode(MatchMode.PlayerVsPlayer);
    }

    public void SelectPlayerVsAI()
    {
        SelectMatchMode(MatchMode.PlayerVsAI);
    }

    public void SelectAIvsAI()
    {
        SelectMatchMode(MatchMode.AIVsAI);
    }

    public void CharacterSelect()
    {
        CloseSettings();
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = false;

        mainMenuPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        BeginCharacterSelection();
        Focus(characterSelectPanel);
    }

    public void TrainingRoom()
    {
        CloseSettings();
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = true;

        mainMenuPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        BeginCharacterSelection();
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
        DeckBuilderUI deckBuilder = cardsSelectPanel.GetComponent<DeckBuilderUI>();
        if (deckBuilder != null)
            deckBuilder.BeginSelection(PlayerSlot.PlayerOne);
        Focus(cardsSelectPanel);
    }

    public void HandleCharacterSelectionComplete()
    {
        if (SelectionManager.Instance != null && !ModeRules.UsesCustomDeck(SelectionManager.Instance.matchMode, PlayerSlot.PlayerOne))
        {
            ShowMapSelect();
            return;
        }

        ShowCardsSelect();
    }

    public void ShowMapSelect()
    {
        CloseSettings();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        mapSelectPanel.SetActive(true);
        Focus(mapSelectPanel);
    }

    public void BackToCharacterSelect()
    {
        CloseSettings();
        cardsSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        BeginCharacterSelection();
        Focus(characterSelectPanel);
    }

    public void BackToCardsSelect()
    {
        CloseSettings();

        if (SelectionManager.Instance != null && SelectionManager.Instance.matchMode == MatchMode.AIVsAI)
        {
            BackToCharacterSelect();
            return;
        }

        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(true);
        Focus(cardsSelectPanel);
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
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (cardsSelectPanel != null) cardsSelectPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
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

    private void SelectMatchMode(MatchMode mode)
    {
        if (SelectionManager.Instance == null)
        {
            Debug.LogError("[MenuManager] SelectionManager is required before choosing a match mode.", this);
            return;
        }

        SelectionManager.Instance.BeginMatchSetup(mode);
        DeckManager.Instance?.ClearDecks();
        CharacterSelect();
    }

    private void BeginCharacterSelection()
    {
        if (characterSelectPanel == null)
            return;

        CharacterSelectUI characterSelect = characterSelectPanel.GetComponent<CharacterSelectUI>();
        if (characterSelect != null)
            characterSelect.BeginSelection();
    }

    private void Focus(GameObject panel)
    {
        if (panel != null)
            StartCoroutine(UIFocus.SelectFirstNextFrame(panel));
    }

}
