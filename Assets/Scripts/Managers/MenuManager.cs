using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameModePanel;
    public GameObject rulesPanel;
    [SerializeField] private MatchRulesPanel rulesView;
    public GameObject howToPlayPanel;
    public GameObject controlsPanel;
    public GameObject characterSelectPanel;
    public GameObject cardsSelectPanel;
    public GameObject mapSelectPanel;
    public GameObject settingsPanel;

    [Header("Panel Transition")]
    [SerializeField, Min(0f)] private float transitionSeconds = 0.3f;
    [SerializeField, Min(0f)] private float confirmationHoldSeconds = 0.2f;

    private InputAction cancelAction;
    private InputAction showRulesAction;
    private bool backQueued;
    private bool transitioning;
    private GameObject currentPanel;

    private void Start()
    {
        showRulesAction = InputSystem.actions?.FindAction("UI/ShowRules");
        if (EventSystem.current != null &&
            EventSystem.current.TryGetComponent(out InputSystemUIInputModule inputModule))
            cancelAction = inputModule.cancel?.action;

        GameObject[] panels = { mainMenuPanel, gameModePanel, rulesPanel, howToPlayPanel,
            controlsPanel, characterSelectPanel, cardsSelectPanel, mapSelectPanel, settingsPanel };
        foreach (GameObject panel in panels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        ShowMainMenu();
    }

    private void Update()
    {
        if (transitioning)
            return;

        if (gameModePanel != null && gameModePanel.activeSelf && rulesView != null &&
            showRulesAction != null && showRulesAction.WasPressedThisFrame())
        {
            ShowRules();
            return;
        }

        if (backQueued || cancelAction == null || !cancelAction.WasPressedThisFrame())
            return;

        StartCoroutine(BackAfterUiCancel());
    }

    private IEnumerator BackAfterUiCancel()
    {
        backQueued = true;
        // Let the EventSystem finish dispatching Cancel before changing the selected panel.
        yield return null;
        backQueued = false;

        if (transitioning)
            yield break;

        if (cardsSelectPanel != null && cardsSelectPanel.activeInHierarchy)
        {
            DeckBuilderUI deckBuilder = cardsSelectPanel.GetComponent<DeckBuilderUI>();
            if (deckBuilder != null &&
                (deckBuilder.CloseCardInfo() || deckBuilder.TryRemoveLastCard()))
                yield break;
        }

        Back();
    }

    public void Back()
    {
        if (transitioning)
            return;

        if (rulesPanel != null && rulesPanel.activeSelf)
            ShowGameModeSelect();
        else if (settingsPanel != null && settingsPanel.activeSelf)
        {
            SettingsController settings = settingsPanel.GetComponent<SettingsController>();
            if (settings == null || !settings.TryGoBack())
                HideSettings();
        }
        else if (controlsPanel != null && controlsPanel.activeSelf)
            HideControls();
        else if (howToPlayPanel != null && howToPlayPanel.activeSelf)
            ShowMainMenu();
        else if (mapSelectPanel != null && mapSelectPanel.activeSelf)
        {
            StageSelectionUI stageSelection = mapSelectPanel.GetComponent<StageSelectionUI>();
            if (stageSelection == null || !stageSelection.TryReturnToMapSelection())
                BackToCardsSelect();
        }
        else if (cardsSelectPanel != null && cardsSelectPanel.activeSelf)
        {
            DeckBuilderUI deckBuilder = cardsSelectPanel.GetComponent<DeckBuilderUI>();
            if (deckBuilder == null || !deckBuilder.CloseCardInfo())
                BackFromCardsSelect();
        }
        else if (characterSelectPanel != null && characterSelectPanel.activeSelf)
        {
            if (SelectionManager.Instance != null && SelectionManager.Instance.isTrainingMode)
                ShowMainMenu();
            else
                ShowGameModeSelect();
        }
        else if (gameModePanel != null && gameModePanel.activeSelf)
            ShowMainMenu();
    }

    public void BackFromCardsSelect()
    {
        if (transitioning)
            return;

        if (cardsSelectPanel == null || !cardsSelectPanel.activeSelf)
            return;

        DeckBuilderUI deckBuilder = cardsSelectPanel.GetComponent<DeckBuilderUI>();
        if (deckBuilder != null && deckBuilder.TryReturnToPlayerOneDeck())
        {
            Focus(cardsSelectPanel);
            return;
        }

        BackToCharacterSelect();
    }

    public void ShowMainMenu()
    {
        SwitchPanel(mainMenuPanel, true);
    }

    public void ShowHowToPlay()
    {
        SwitchPanel(howToPlayPanel);
    }

    public void ShowGameModeSelect()
    {
        bool goingBack = currentPanel != mainMenuPanel;
        SwitchPanel(gameModePanel, goingBack);
    }

    public void ShowRules()
    {
        if (rulesView == null || rulesPanel == null || gameModePanel == null)
            return;

        SwitchPanel(rulesPanel);
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
        if (transitioning)
            return;

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = false;

        SwitchPanel(characterSelectPanel, false, BeginCharacterSelection);
    }

    public void TrainingRoom()
    {
        if (transitioning)
            return;

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = true;

        SwitchPanel(characterSelectPanel, false, BeginCharacterSelection);
    }

    public void GoToTrainingRoom()
    {
        SceneManager.LoadScene("TrainingRoom");
    }

    public void ShowCardsSelect()
    {
        SwitchPanel(cardsSelectPanel, false, () =>
        {
            DeckBuilderUI deckBuilder = cardsSelectPanel.GetComponent<DeckBuilderUI>();
            if (deckBuilder != null)
                deckBuilder.BeginSelection(PlayerSlot.PlayerOne);
        });
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
        SwitchPanel(mapSelectPanel);
    }

    public void BackToCharacterSelect()
    {
        SwitchPanel(characterSelectPanel, true, BeginCharacterSelection);
    }

    public void BackToCardsSelect()
    {
        if (SelectionManager.Instance != null && SelectionManager.Instance.matchMode == MatchMode.AIVsAI)
        {
            BackToCharacterSelect();
            return;
        }

        SwitchPanel(cardsSelectPanel, true);
    }

    public void GoToStage1()
    {
        LoadingScreen.LoadStage("Stage1");
    }

    public void ShowControls()
    {
        SwitchPanel(controlsPanel);
    }

    public void HideControls()
    {
        SwitchPanel(howToPlayPanel, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowSettings()
    {
        if (settingsPanel != null && settingsPanel.TryGetComponent(out SettingsController settings))
            settings.AnimateSections = true;
        SwitchPanel(settingsPanel);
    }

    public void HideSettings()
    {
        ShowMainMenu();
    }

    private void SelectMatchMode(MatchMode mode)
    {
        if (transitioning)
            return;

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

    private void SwitchPanel(GameObject target, bool goingBack = false, Action onEnter = null)
    {
        if (target == null || transitioning)
            return;

        if (currentPanel == settingsPanel && settingsPanel != null &&
            settingsPanel.TryGetComponent(out SettingsController settings) && settings.IsSectionChanging)
            return;

        if (currentPanel == target)
        {
            Focus(target);
            return;
        }

        if (currentPanel == null || transitionSeconds <= 0f)
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);
            target.SetActive(true);
            currentPanel = target;
            onEnter?.Invoke();
            Focus(target);
            return;
        }

        StartCoroutine(TransitionPanels(currentPanel, target, goingBack ? -1f : 1f, onEnter));
    }

    private IEnumerator TransitionPanels(GameObject outgoing, GameObject incoming, float direction, Action onEnter)
    {
        transitioning = true;

        // Let the first flashes read before the selected panel starts moving.
        if (confirmationHoldSeconds > 0f)
            yield return new WaitForSecondsRealtime(confirmationHoldSeconds);

        UIFocus.Clear();

        CanvasGroup outgoingGroup = GetTransitionGroup(outgoing);
        CanvasGroup incomingGroup = GetTransitionGroup(incoming);
        RectTransform outgoingRect = outgoing.GetComponent<RectTransform>();
        RectTransform incomingRect = incoming.GetComponent<RectTransform>();
        Vector2 outgoingPosition = outgoingRect.anchoredPosition;
        Vector2 incomingPosition = incomingRect.anchoredPosition;
        float travel = Mathf.Max(outgoingRect.rect.width, incomingRect.rect.width);
        bool outgoingInteractable = outgoingGroup.interactable;
        bool outgoingBlocksRaycasts = outgoingGroup.blocksRaycasts;
        bool incomingInteractable = incomingGroup.interactable;
        bool incomingBlocksRaycasts = incomingGroup.blocksRaycasts;

        outgoingGroup.interactable = false;
        outgoingGroup.blocksRaycasts = false;
        incoming.SetActive(true);
        onEnter?.Invoke();
        UIFocus.Clear();
        incomingGroup.interactable = false;
        incomingGroup.blocksRaycasts = false;
        incomingRect.anchoredPosition = incomingPosition + Vector2.right * (direction * travel);

        float elapsed = 0f;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionSeconds);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            outgoingRect.anchoredPosition = outgoingPosition - Vector2.right * (direction * travel * eased);
            incomingRect.anchoredPosition = incomingPosition + Vector2.right * (direction * travel * (1f - eased));
            yield return null;
        }

        outgoingRect.anchoredPosition = outgoingPosition;
        outgoingGroup.interactable = outgoingInteractable;
        outgoingGroup.blocksRaycasts = outgoingBlocksRaycasts;
        outgoing.SetActive(false);

        incomingRect.anchoredPosition = incomingPosition;
        incomingGroup.interactable = incomingInteractable;
        incomingGroup.blocksRaycasts = incomingBlocksRaycasts;
        currentPanel = incoming;
        transitioning = false;
        Focus(incoming);
    }

    private static CanvasGroup GetTransitionGroup(GameObject panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        return group != null ? group : panel.AddComponent<CanvasGroup>();
    }

}
