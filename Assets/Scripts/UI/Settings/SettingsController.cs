using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public bool AnimateSections { get; set; }
    public bool IsSectionChanging => sectionChanging;

    [Header("Sections")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("Control Schemes")]
    [SerializeField] private GameObject keyboardControlsPanel;
    [SerializeField] private GameObject gamepadControlsPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsVolumeSlider;
    [SerializeField] private Slider rumbleIntensitySlider;
    [SerializeField] private Slider screenShakeIntensitySlider;

    [Header("Optional Value Labels")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI soundEffectsVolumeText;
    [SerializeField] private TextMeshProUGUI rumbleIntensityText;
    [SerializeField] private TextMeshProUGUI screenShakeIntensityText;

    [Header("Gameplay Aids")]
    [SerializeField] private Toggle lastCardToggle;
    [SerializeField] private Toggle abilityIconsToggle;
    [SerializeField] private Toggle inputHistoryToggle;
    [SerializeField] private Toggle matchTutorialToggle;

    [Header("Optional Initial Selection")]
    [SerializeField] private Selectable settingsInitialSelection;
    [SerializeField] private Selectable audioInitialSelection;
    [SerializeField] private Selectable controlsInitialSelection;
    [SerializeField] private Selectable keyboardInitialSelection;
    [SerializeField] private Selectable gamepadInitialSelection;
    [SerializeField] private Selectable gameplayInitialSelection;

    private bool sectionChanging;
    private Coroutine sectionTransition;

    private void OnEnable()
    {
        RefreshAudioControls();
        RefreshGameplayControls();
        SubscribeToSliders();
        SubscribeToGameplayToggles();
        ApplySettingsHome();
        Select(settingsInitialSelection, gameObject);
    }

    private void OnDisable()
    {
        if (sectionTransition != null)
            StopCoroutine(sectionTransition);
        sectionTransition = null;
        sectionChanging = false;

        UnsubscribeFromSliders();
        UnsubscribeFromGameplayToggles();
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
        GameSettings.Save();
    }

    public void ShowSettingsHome()
    {
        ChangeSection(ApplySettingsHome, settingsInitialSelection, gameObject, true);
    }

    public void ShowAudio()
    {
        ChangeSection(() => ApplySection(audioPanel), audioInitialSelection, audioPanel);
    }

    public void ShowControls()
    {
        bool goingBack = (keyboardControlsPanel != null && keyboardControlsPanel.activeSelf) ||
            (gamepadControlsPanel != null && gamepadControlsPanel.activeSelf);
        ChangeSection(() => ApplySection(controlsPanel), controlsInitialSelection, controlsPanel, goingBack);
    }

    public void ShowKeyboardControls()
    {
        ChangeSection(() => ApplySection(keyboardControlsPanel), keyboardInitialSelection, keyboardControlsPanel);
    }

    public void ShowGamepadControls()
    {
        ChangeSection(() => ApplySection(gamepadControlsPanel), gamepadInitialSelection, gamepadControlsPanel);
    }

    public void ShowGameplay()
    {
        if (gameplayPanel == null)
        {
            Debug.LogWarning("[SettingsController] Assign the Gameplay Panel in the Inspector.", this);
            return;
        }

        RefreshGameplayControls();
        ChangeSection(() => ApplySection(gameplayPanel), gameplayInitialSelection, gameplayPanel);
    }

    public bool TryGoBack()
    {
        if (sectionChanging)
            return true;

        if ((keyboardControlsPanel != null && keyboardControlsPanel.activeSelf) ||
            (gamepadControlsPanel != null && gamepadControlsPanel.activeSelf))
        {
            ShowControls();
            return true;
        }

        if ((audioPanel != null && audioPanel.activeSelf) ||
            (controlsPanel != null && controlsPanel.activeSelf) ||
            (gameplayPanel != null && gameplayPanel.activeSelf))
        {
            ShowSettingsHome();
            return true;
        }

        return false;
    }

    public void Back()
    {
        TryGoBack();
    }

    private void ApplySettingsHome()
    {
        SetSettingsHomeContentActive(true);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
    }

    private void ApplySection(GameObject section)
    {
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, section == audioPanel);
        SetActive(controlsPanel, section == controlsPanel);
        SetActive(gameplayPanel, section == gameplayPanel);
        SetActive(keyboardControlsPanel, section == keyboardControlsPanel);
        SetActive(gamepadControlsPanel, section == gamepadControlsPanel);
    }

    private void ChangeSection(Action apply, Selectable initialSelection, GameObject focusRoot, bool goingBack = false)
    {
        if (sectionChanging)
            return;

        GameObject outgoing = GetCurrentSectionRoot();
        if (!AnimateSections || !isActiveAndEnabled || focusRoot == null || outgoing == focusRoot)
        {
            apply();
            Select(initialSelection, focusRoot);
            return;
        }

        sectionTransition = StartCoroutine(TransitionSection(outgoing, focusRoot, apply,
            initialSelection, goingBack ? -1f : 1f));
    }

    private GameObject GetCurrentSectionRoot()
    {
        if (audioPanel != null && audioPanel.activeSelf) return audioPanel;
        if (controlsPanel != null && controlsPanel.activeSelf) return controlsPanel;
        if (gameplayPanel != null && gameplayPanel.activeSelf) return gameplayPanel;
        if (keyboardControlsPanel != null && keyboardControlsPanel.activeSelf) return keyboardControlsPanel;
        if (gamepadControlsPanel != null && gamepadControlsPanel.activeSelf) return gamepadControlsPanel;
        return gameObject;
    }

    private IEnumerator TransitionSection(GameObject outgoing, GameObject incoming, Action apply,
        Selectable initialSelection, float direction)
    {
        sectionChanging = true;
        yield return new WaitForSecondsRealtime(0.2f);

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

        if (incoming == gameObject)
            SetSettingsHomeContentActive(true);
        else
            incoming.SetActive(true);
        UIFocus.Clear();
        outgoingGroup.interactable = false;
        outgoingGroup.blocksRaycasts = false;
        incomingGroup.interactable = false;
        incomingGroup.blocksRaycasts = false;
        incomingRect.anchoredPosition = incomingPosition + Vector2.right * (direction * travel);

        float elapsed = 0f;
        const float slideSeconds = 0.3f;
        while (elapsed < slideSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideSeconds);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            outgoingRect.anchoredPosition = outgoingPosition - Vector2.right * (direction * travel * eased);
            incomingRect.anchoredPosition = incomingPosition + Vector2.right * (direction * travel * (1f - eased));
            yield return null;
        }

        outgoingRect.anchoredPosition = outgoingPosition;
        incomingRect.anchoredPosition = incomingPosition;
        outgoingGroup.interactable = outgoingInteractable;
        outgoingGroup.blocksRaycasts = outgoingBlocksRaycasts;
        incomingGroup.interactable = incomingInteractable;
        incomingGroup.blocksRaycasts = incomingBlocksRaycasts;
        apply();
        sectionChanging = false;
        sectionTransition = null;
        Select(initialSelection, incoming);
    }

    private static CanvasGroup GetTransitionGroup(GameObject panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        return group != null ? group : panel.AddComponent<CanvasGroup>();
    }

    public void SetMasterVolume(float value)
    {
        GameSettings.SetMasterVolume(value);
        SetPercent(masterVolumeText, value);
    }

    public void SetMusicVolume(float value)
    {
        GameSettings.SetMusicVolume(value);
        SetPercent(musicVolumeText, value);
    }

    public void SetSoundEffectsVolume(float value)
    {
        GameSettings.SetSoundEffectsVolume(value);
        SetPercent(soundEffectsVolumeText, value);
    }

    public void SetRumbleIntensity(float value)
    {
        GameSettings.SetRumbleIntensity(value);
        SetPercent(rumbleIntensityText, value);
    }

    public void SetScreenShakeIntensity(float value)
    {
        GameSettings.SetScreenShakeIntensity(value);
        SetPercent(screenShakeIntensityText, value);
    }

    public void ResetAudioToDefaults()
    {
        GameSettings.ResetToDefaults();
        RefreshAudioControls();
    }

    public void RefreshAudioControls()
    {
        SetSliderValue(masterVolumeSlider, GameSettings.MasterVolume);
        SetSliderValue(musicVolumeSlider, GameSettings.MusicVolume);
        SetSliderValue(soundEffectsVolumeSlider, GameSettings.SoundEffectsVolume);
        SetSliderValue(rumbleIntensitySlider, GameSettings.RumbleIntensity);
        SetSliderValue(screenShakeIntensitySlider, GameSettings.ScreenShakeIntensity);

        SetPercent(masterVolumeText, GameSettings.MasterVolume);
        SetPercent(musicVolumeText, GameSettings.MusicVolume);
        SetPercent(soundEffectsVolumeText, GameSettings.SoundEffectsVolume);
        SetPercent(rumbleIntensityText, GameSettings.RumbleIntensity);
        SetPercent(screenShakeIntensityText, GameSettings.ScreenShakeIntensity);
    }

    public void RefreshGameplayControls()
    {
        SetToggleValue(lastCardToggle, GameplayAidSettings.ShowLastCard);
        SetToggleValue(abilityIconsToggle, GameplayAidSettings.ShowAbilityIcons);
        SetToggleValue(inputHistoryToggle, GameplayAidSettings.ShowInputHistory);
        SetToggleValue(matchTutorialToggle, GameplayAidSettings.ShowMatchTutorial);
    }

    private void SubscribeToGameplayToggles()
    {
        if (lastCardToggle != null) lastCardToggle.onValueChanged.AddListener(GameplayAidSettings.SetShowLastCard);
        if (abilityIconsToggle != null) abilityIconsToggle.onValueChanged.AddListener(GameplayAidSettings.SetShowAbilityIcons);
        if (inputHistoryToggle != null) inputHistoryToggle.onValueChanged.AddListener(GameplayAidSettings.SetShowInputHistory);
        if (matchTutorialToggle != null) matchTutorialToggle.onValueChanged.AddListener(GameplayAidSettings.SetShowMatchTutorial);
    }

    private void UnsubscribeFromGameplayToggles()
    {
        if (lastCardToggle != null) lastCardToggle.onValueChanged.RemoveListener(GameplayAidSettings.SetShowLastCard);
        if (abilityIconsToggle != null) abilityIconsToggle.onValueChanged.RemoveListener(GameplayAidSettings.SetShowAbilityIcons);
        if (inputHistoryToggle != null) inputHistoryToggle.onValueChanged.RemoveListener(GameplayAidSettings.SetShowInputHistory);
        if (matchTutorialToggle != null) matchTutorialToggle.onValueChanged.RemoveListener(GameplayAidSettings.SetShowMatchTutorial);
    }

    private static void SetToggleValue(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private void SubscribeToSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (soundEffectsVolumeSlider != null)
        {
            soundEffectsVolumeSlider.onValueChanged.AddListener(SetSoundEffectsVolume);
        }

        if (rumbleIntensitySlider != null)
        {
            rumbleIntensitySlider.onValueChanged.AddListener(SetRumbleIntensity);
        }

        if (screenShakeIntensitySlider != null)
        {
            screenShakeIntensitySlider.onValueChanged.AddListener(SetScreenShakeIntensity);
        }
    }

    private void UnsubscribeFromSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }

        if (soundEffectsVolumeSlider != null)
        {
            soundEffectsVolumeSlider.onValueChanged.RemoveListener(SetSoundEffectsVolume);
        }

        if (rumbleIntensitySlider != null)
        {
            rumbleIntensitySlider.onValueChanged.RemoveListener(SetRumbleIntensity);
        }

        if (screenShakeIntensitySlider != null)
        {
            screenShakeIntensitySlider.onValueChanged.RemoveListener(SetScreenShakeIntensity);
        }
    }

    private static void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }

    private static void SetPercent(TextMeshProUGUI label, float value)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    private void SetSettingsHomeContentActive(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            SetActive(transform.GetChild(i).gameObject, active);
        }
    }

    private static void Select(Selectable selectable, GameObject fallbackRoot)
    {
        if (selectable != null && selectable.IsActive() && selectable.IsInteractable())
        {
            selectable.Select();
            return;
        }

        UIFocus.SelectFirst(fallbackRoot);
    }
}
