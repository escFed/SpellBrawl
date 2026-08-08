using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;

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

    [Header("Optional Initial Selection")]
    [SerializeField] private Selectable audioInitialSelection;
    [SerializeField] private Selectable keyboardInitialSelection;
    [SerializeField] private Selectable gamepadInitialSelection;

    private bool gamepadControlsSelected;

    private void OnEnable()
    {
        RefreshAudioControls();
        SubscribeToSliders();
        ShowAudio();
    }

    private void OnDisable()
    {
        UnsubscribeFromSliders();
        GameSettings.Save();
    }

    public void ShowAudio()
    {
        SetActive(audioPanel, true);
        SetActive(controlsPanel, false);
        Select(audioInitialSelection);
    }

    public void ShowControls()
    {
        SetActive(audioPanel, false);
        SetActive(controlsPanel, true);

        if (gamepadControlsSelected)
        {
            ShowGamepadControls();
        }
        else
        {
            ShowKeyboardControls();
        }
    }

    public void ShowKeyboardControls()
    {
        gamepadControlsSelected = false;
        SetActive(keyboardControlsPanel, true);
        SetActive(gamepadControlsPanel, false);
        Select(keyboardInitialSelection);
    }

    public void ShowGamepadControls()
    {
        gamepadControlsSelected = true;
        SetActive(keyboardControlsPanel, false);
        SetActive(gamepadControlsPanel, true);
        Select(gamepadInitialSelection);
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

        SetPercent(masterVolumeText, GameSettings.MasterVolume);
        SetPercent(musicVolumeText, GameSettings.MusicVolume);
        SetPercent(soundEffectsVolumeText, GameSettings.SoundEffectsVolume);
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

    private static void Select(Selectable selectable)
    {
        if (selectable != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
}
