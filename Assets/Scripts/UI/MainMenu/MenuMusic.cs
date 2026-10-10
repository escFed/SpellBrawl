using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    private static MenuMusic instance;

    private AudioSource source;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        instance = null;
    }

    public static void StartFromBootstrap(AudioClip clip)
    {
        if (instance != null || clip == null)
            return;

        GameObject musicObject = new GameObject("Menu Music");
        AudioSource musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.clip = clip;

        MenuMusic music = musicObject.AddComponent<MenuMusic>();
        music.Play();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        source = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
        GameSettings.RegisterSource(source, GameSound.Music);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            Play();
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
    {
        if (nextScene.name == "MainMenu")
            Play();
        else if (nextScene.name != "Bootstrap")
            source.Stop();
    }

    private void Play()
    {
        if (instance == this && source != null && source.clip != null && !source.isPlaying)
            source.Play();
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        instance = null;
    }
}
