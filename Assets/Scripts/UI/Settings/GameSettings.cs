using UnityEngine;
using UnityEngine.SceneManagement;
public static class GameSettings
{
    private const string MasterVolumeKey = "audio.masterVolume";
    private const string MusicVolumeKey = "audio.musicVolume";
    private const string SoundEffectsVolumeKey = "audio.soundEffectsVolume";

    private const float DefaultVolume = 1f;

    public static event System.Action VolumesChanged;

    public static float MasterVolume { get; private set; } = DefaultVolume;
    public static float MusicVolume { get; private set; } = DefaultVolume;
    public static float SoundEffectsVolume { get; private set; } = DefaultVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        VolumesChanged = null;
        MasterVolume = DefaultVolume;
        MusicVolume = DefaultVolume;
        SoundEffectsVolume = DefaultVolume;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        SoundEffectsVolume = PlayerPrefs.GetFloat(SoundEffectsVolumeKey, DefaultVolume);

        ApplyMasterVolume();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        ApplyMasterVolume();
        VolumesChanged?.Invoke();
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        VolumesChanged?.Invoke();
    }

    public static void SetSoundEffectsVolume(float value)
    {
        SoundEffectsVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SoundEffectsVolumeKey, SoundEffectsVolume);
        VolumesChanged?.Invoke();
    }

    public static void ResetToDefaults()
    {
        MasterVolume = DefaultVolume;
        MusicVolume = DefaultVolume;
        SoundEffectsVolume = DefaultVolume;

        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SoundEffectsVolumeKey, SoundEffectsVolume);

        ApplyMasterVolume();
        VolumesChanged?.Invoke();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void RegisterSource(AudioSource source, GameSound sound)
    {
        AudioController.Register(source, sound);
    }

    public static void RegisterLoadedSceneSources()
    {
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            RegisterSceneSources(SceneManager.GetSceneAt(sceneIndex));
        }
    }

    private static void ApplyMasterVolume()
    {
        AudioListener.volume = MasterVolume;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        RegisterSceneSources(scene);
    }

    private static void RegisterSceneSources(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
        {
            AudioSource[] sources = rootObjects[rootIndex].GetComponentsInChildren<AudioSource>(true);
            for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
            {
                AudioSource source = sources[sourceIndex];
                GameSound sound = source.loop
                    ? GameSound.Music
                    : GameSound.SoundEffects;

                RegisterSource(source, sound);
            }
        }
    }
}
