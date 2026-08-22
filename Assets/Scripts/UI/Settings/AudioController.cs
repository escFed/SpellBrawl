using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    [SerializeField] private GameSound sound = GameSound.SoundEffects;
    [SerializeField] private float authoredVolume = -1f;

    private AudioSource audioSource;

    public static AudioController Register(AudioSource source, GameSound desiredSound)
    {
        if (source == null)
        {
            return null;
        }

        if (!source.TryGetComponent(out AudioController channelController))
        {
            channelController = source.gameObject.AddComponent<AudioController>();
        }

        channelController.Configure(desiredSound);
        return channelController;
    }

    private void Awake()
    {
        CacheSourceAndVolume();
    }

    private void OnEnable()
    {
        CacheSourceAndVolume();
        GameSettings.VolumesChanged += ApplyVolume;
        ApplyVolume();
    }

    private void OnDisable()
    {
        GameSettings.VolumesChanged -= ApplyVolume;
    }

    private void Configure(GameSound desiredSound)
    {
        CacheSourceAndVolume();
        sound = desiredSound;
        ApplyVolume();
    }

    private void CacheSourceAndVolume()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null && authoredVolume < 0f)
        {
            authoredVolume = audioSource.volume;
        }
    }

    private void ApplyVolume()
    {
        if (audioSource == null)
        {
            return;
        }

        float channelVolume = sound == GameSound.Music ? GameSettings.MusicVolume : GameSettings.SoundEffectsVolume;

        audioSource.volume = Mathf.Max(0f, authoredVolume) * channelVolume;
    }
}
