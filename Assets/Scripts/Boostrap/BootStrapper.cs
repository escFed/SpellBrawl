using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootStrapper : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float minimumBlackSeconds = 2f;
    [SerializeField, Min(0f)] private float recommendationFadeSeconds = 0.7f;
    [SerializeField, Min(0f)] private float recommendationHoldSeconds = 3.5f;
    [SerializeField, Min(0f)] private float titleFadeSeconds = 0.7f;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup recommendationGroup;
    [SerializeField] private CanvasGroup blackCover;
    [SerializeField] private Image loadingIcon;

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusicClip;

    private InputAction continueAction;

    private void Awake()
    {
        continueAction = InputSystem.actions?.FindAction("UI/Continue");
        if (titleGroup != null)
            titleGroup.alpha = 0f;
        if (recommendationGroup != null)
            recommendationGroup.alpha = 0f;
        if (blackCover != null)
            blackCover.alpha = 1f;
        if (loadingIcon != null)
            loadingIcon.color = Color.white;
    }

    private void Start()
    {
        StartCoroutine(ShowTitleWhenReady());
    }

    private IEnumerator ShowTitleWhenReady()
    {
        if (titleGroup == null || recommendationGroup == null || blackCover == null || loadingIcon == null ||
            !Application.CanStreamedLevelBeLoaded("MainMenu") || continueAction == null)
        {
            Debug.LogError("[BootStrapper] Assign the Canvas, UI/Continue action, and MainMenu build scene.", this);
            yield break;
        }

        float startedAt = Time.realtimeSinceStartup;
        yield return null; // Render the black Canvas before beginning the menu load.

        AsyncOperation menuLoad = SceneManager.LoadSceneAsync("MainMenu");
        menuLoad.allowSceneActivation = false;

        while (menuLoad.progress < 0.9f || Time.realtimeSinceStartup - startedAt < minimumBlackSeconds)
            yield return null;

        float elapsed = 0f;
        while (elapsed < recommendationFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, recommendationFadeSeconds));
            recommendationGroup.alpha = progress;
            loadingIcon.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }
        recommendationGroup.alpha = 1f;
        loadingIcon.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(recommendationHoldSeconds);

        // The splash fully covers the screen, so the black layer can be removed underneath it.
        blackCover.alpha = 0f;
        MenuMusic.StartFromBootstrap(menuMusicClip);

        elapsed = 0f;
        while (elapsed < titleFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, titleFadeSeconds));
            titleGroup.alpha = progress;
            recommendationGroup.alpha = 1f - progress;
            yield return null;
        }
        titleGroup.alpha = 1f;
        recommendationGroup.alpha = 0f;
        recommendationGroup.gameObject.SetActive(false);

        while (!continueAction.WasPressedThisFrame())
            yield return null;

        menuLoad.allowSceneActivation = true;
    }
}
