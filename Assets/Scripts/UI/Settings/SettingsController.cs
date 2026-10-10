using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
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

    [Header("Optional Value Labels")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI soundEffectsVolumeText;

    [Header("Gameplay Aids")]
    [SerializeField] private Toggle lastCardToggle;
    [SerializeField] private Toggle abilityIconsToggle;
    [SerializeField] private Toggle inputHistoryToggle;
    [SerializeField] private Toggle matchTutorialToggle;

    private Slider rumbleIntensitySlider;
    private TextMeshProUGUI rumbleIntensityText;

    [Header("Optional Initial Selection")]
    [SerializeField] private Selectable settingsInitialSelection;
    [SerializeField] private Selectable audioInitialSelection;
    [SerializeField] private Selectable controlsInitialSelection;
    [SerializeField] private Selectable keyboardInitialSelection;
    [SerializeField] private Selectable gamepadInitialSelection;
    [SerializeField] private Selectable gameplayInitialSelection;

    private void OnEnable()
    {
        EnsureRumbleControl();
        RefreshAudioControls();
        RefreshGameplayControls();
        SubscribeToSliders();
        SubscribeToGameplayToggles();
        ShowSettingsHome();
    }

    private void OnDisable()
    {
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
        SetSettingsHomeContentActive(true);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
        Select(settingsInitialSelection, gameObject);
    }

    public void ShowAudio()
    {
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, true);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
        Select(audioInitialSelection, audioPanel);
    }

    public void ShowControls()
    {
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, true);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
        Select(controlsInitialSelection, controlsPanel);
    }

    public void ShowKeyboardControls()
    {
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, true);
        SetActive(gamepadControlsPanel, false);
        Select(keyboardInitialSelection, keyboardControlsPanel);
    }

    public void ShowGamepadControls()
    {
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(gameplayPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, true);
        Select(gamepadInitialSelection, gamepadControlsPanel);
    }

    public void ShowGameplay()
    {
        if (gameplayPanel == null)
        {
            Debug.LogWarning("[SettingsController] Assign the Gameplay Panel in the Inspector.", this);
            return;
        }

        RefreshGameplayControls();
        SetSettingsHomeContentActive(false);
        SetActive(audioPanel, false);
        SetActive(controlsPanel, false);
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, false);
        SetActive(gameplayPanel, true);
        Select(gameplayInitialSelection, gameplayPanel);
    }

    public bool TryGoBack()
    {
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

        SetPercent(masterVolumeText, GameSettings.MasterVolume);
        SetPercent(musicVolumeText, GameSettings.MusicVolume);
        SetPercent(soundEffectsVolumeText, GameSettings.SoundEffectsVolume);
        SetPercent(rumbleIntensityText, GameSettings.RumbleIntensity);
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

    private void EnsureRumbleControl()
    {
        if (rumbleIntensitySlider != null || audioPanel == null || soundEffectsVolumeSlider == null)
            return;

        // Both settings screens use the same authored three-row layout.
        // Clone its last row so the new option has the existing visuals and navigation.
        rumbleIntensitySlider = Instantiate(soundEffectsVolumeSlider, audioPanel.transform);
        rumbleIntensitySlider.name = "RumbleSlider";
        rumbleIntensitySlider.onValueChanged.RemoveAllListeners();

        TextMeshProUGUI[] sourceTexts = soundEffectsVolumeSlider.GetComponentsInChildren<TextMeshProUGUI>(true);
        TextMeshProUGUI[] rumbleTexts = rumbleIntensitySlider.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < Mathf.Min(sourceTexts.Length, rumbleTexts.Length); i++)
        {
            if (sourceTexts[i] == soundEffectsVolumeText)
                rumbleIntensityText = rumbleTexts[i];
            else
                rumbleTexts[i].text = "VIBRATION";
        }

        SetRowPosition(masterVolumeSlider, 225f);
        SetRowPosition(musicVolumeSlider, 75f);
        SetRowPosition(soundEffectsVolumeSlider, -75f);
        SetRowPosition(rumbleIntensitySlider, -225f);
    }

    private static void SetRowPosition(Slider slider, float y)
    {
        if (slider != null && slider.transform is RectTransform rect)
        {
            Vector2 position = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(position.x, y);
        }
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
