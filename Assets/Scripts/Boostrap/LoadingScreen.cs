using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float minimumDisplaySeconds = 5f;
    [SerializeField, Min(0.1f)] private float imageIntervalSeconds = 2.4f;
    [SerializeField, Min(0f)] private float imageFadeSeconds = 0.7f;
    [SerializeField, Min(0f)] private float transitionSeconds = 0.4f;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup screenGroup;
    [SerializeField] private Image firstBackground;
    [SerializeField] private Image secondBackground;
    [SerializeField] private Sprite[] stageBackgrounds;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private CanvasGroup blackTransition;

    [Header("Tips")]
    [SerializeField] private string[] tips =
    {
        "TIP: USE YOUR CARDS AT THE RIGHT MOMENT.",
        "TIP: A WELL-TIMED DODGE CAN TURN THE TIDE OF BATTLE.",
        "TIP: TRY OUT DIFFERENT CARDS TO FIND YOUR STRATEGY.",
        "TIP: USE YOUR SHIELD TO WITHSTAND ATTACKS."
    };

    private static string pendingStage;
    private static bool transitionQueued;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        pendingStage = null;
        transitionQueued = false;
    }

    public static void LoadStage(string sceneName)
    {
        if (transitionQueued)
            return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName) ||
            !Application.CanStreamedLevelBeLoaded("CombatLoading"))
        {
            Debug.LogError($"[LoadingScreen] Missing Build Settings scene: {sceneName} or CombatLoading.");
            return;
        }

        transitionQueued = true;
        pendingStage = sceneName;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("CombatLoading");
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(pendingStage))
        {
            Debug.LogError("[LoadingScreen] Open a stage from the menu or Rematch.", this);
            return;
        }

        DontDestroyOnLoad(gameObject);
        if (screenGroup != null)
            screenGroup.alpha = 1f;
        if (blackTransition != null)
            blackTransition.alpha = 1f;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(pendingStage) || screenGroup == null || firstBackground == null ||
            secondBackground == null || blackTransition == null ||
            stageBackgrounds == null || stageBackgrounds.Length < 2)
        {
            Debug.LogError("[LoadingScreen] Complete the Canvas and background references.", this);
            return;
        }

        firstBackground.sprite = stageBackgrounds[0];
        secondBackground.color = new Color(1f, 1f, 1f, 0f);
        if (tipText != null && tips != null && tips.Length > 0)
            tipText.text = tips[Random.Range(0, tips.Length)];

        StartCoroutine(CycleBackgrounds());
        StartCoroutine(LoadStageRoutine(pendingStage));
        pendingStage = null;
    }

    private IEnumerator CycleBackgrounds()
    {
        Image front = firstBackground;
        Image next = secondBackground;
        int nextIndex = 1;

        while (true)
        {
            yield return new WaitForSecondsRealtime(imageIntervalSeconds);
            next.sprite = stageBackgrounds[nextIndex];
            next.transform.SetAsLastSibling();
            next.color = new Color(1f, 1f, 1f, 0f);

            float elapsed = 0f;
            while (elapsed < imageFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                next.color = new Color(1f, 1f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, imageFadeSeconds)));
                yield return null;
            }
            next.color = Color.white;
            front.color = new Color(1f, 1f, 1f, 0f);

            Image previous = front;
            front = next;
            next = previous;
            nextIndex = (nextIndex + 1) % stageBackgrounds.Length;
        }
    }

    private IEnumerator LoadStageRoutine(string sceneName)
    {
        float startedAt = Time.realtimeSinceStartup;
        yield return FadeBlack(1f, 0f);

        AsyncOperation stageLoad = SceneManager.LoadSceneAsync(sceneName);
        stageLoad.allowSceneActivation = false;
        while (stageLoad.progress < 0.9f || Time.realtimeSinceStartup - startedAt < minimumDisplaySeconds)
            yield return null;

        yield return FadeBlack(0f, 1f);
        stageLoad.allowSceneActivation = true;
        while (!stageLoad.isDone)
            yield return null;

        yield return null;
        yield return FadeBlack(1f, 0f);
        Destroy(gameObject);
    }

    private IEnumerator FadeBlack(float from, float to)
    {
        float elapsed = 0f;
        blackTransition.alpha = from;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            blackTransition.alpha = Mathf.Lerp(from, to,
                Mathf.Clamp01(elapsed / Mathf.Max(0.001f, transitionSeconds)));
            yield return null;
        }
        blackTransition.alpha = to;
    }

    private void OnDestroy()
    {
        transitionQueued = false;
    }
}
