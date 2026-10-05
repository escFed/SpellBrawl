using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingIconAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;

    private Image icon;
    private float elapsed;

    private void Awake()
    {
        icon = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (icon == null)
            icon = GetComponent<Image>();
        elapsed = 0f;
        if (frames != null && frames.Length > 0)
            icon.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
            return;

        elapsed += Time.unscaledDeltaTime;
        icon.sprite = frames[(int)(elapsed * framesPerSecond) % frames.Length];
    }
}
