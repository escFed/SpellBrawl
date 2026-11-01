using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    private const double ScheduleLeadTime = 0.2;
    private const double LoopScheduleLookahead = 2.0;

    private static MenuMusic instance;

    [Header("Menu music loop (seconds)")]
    [SerializeField, Min(0f)] private float loopStartSeconds = 20f;
    [SerializeField, Min(0f)] private float loopEndSeconds = 92f;

    private AudioSource source;
    private AudioSource loopSource;
    private double nextLoopTime;
    private bool isScheduled;
    private bool useLoopRegion;
    private bool nextLoopUsesSecondSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        instance = null;
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
        source.loop = false;

        GameObject loopPlayer = new GameObject("Menu Music Loop Player");
        loopPlayer.transform.SetParent(transform, false);
        loopSource = loopPlayer.AddComponent<AudioSource>();
        loopSource.clip = source.clip;
        loopSource.playOnAwake = false;
        loopSource.loop = false;
        loopSource.volume = source.volume;
        loopSource.pitch = source.pitch;
        loopSource.spatialBlend = source.spatialBlend;
        loopSource.outputAudioMixerGroup = source.outputAudioMixerGroup;

        DontDestroyOnLoad(gameObject);
        GameSettings.RegisterSource(source, GameSound.Music);
        GameSettings.RegisterSource(loopSource, GameSound.Music);
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
            Stop();
    }

    private void Update()
    {
        if (!isScheduled || !useLoopRegion || AudioSettings.dspTime + LoopScheduleLookahead < nextLoopTime)
            return;

        AudioSource nextSource = nextLoopUsesSecondSource ? loopSource : source;
        nextSource.PlayScheduled(nextLoopTime);
        nextSource.timeSamples = Mathf.RoundToInt(loopStartSeconds * nextSource.clip.frequency);
        nextSource.SetScheduledEndTime(nextLoopTime + loopEndSeconds - loopStartSeconds);

        nextLoopTime += loopEndSeconds - loopStartSeconds;
        nextLoopUsesSecondSource = !nextLoopUsesSecondSource;
    }

    private void Play()
    {
        if (instance != this || source == null || source.clip == null || isScheduled)
            return;

        useLoopRegion = loopStartSeconds >= 0f &&
                        loopEndSeconds > loopStartSeconds &&
                        loopEndSeconds <= source.clip.length;

        if (!useLoopRegion)
        {
            Debug.LogWarning("[MenuMusic] Invalid loop region; playing the entire clip on repeat.", this);
            source.loop = true;
            source.Play();
            isScheduled = true;
            return;
        }

        source.loop = false;
        source.timeSamples = 0;
        double startTime = AudioSettings.dspTime + ScheduleLeadTime;
        source.PlayScheduled(startTime);
        source.SetScheduledEndTime(startTime + loopEndSeconds);
        nextLoopTime = startTime + loopEndSeconds;
        nextLoopUsesSecondSource = true;
        isScheduled = true;
    }

    private void Stop()
    {
        source.Stop();
        loopSource.Stop();
        isScheduled = false;
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        Stop();
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        instance = null;
    }
}
