using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootStrapper : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float minimumBlackSeconds = 2f;
    [SerializeField, Min(0f)] private float titleFadeSeconds = 0.7f;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup blackCover;
    [SerializeField] private Image loadingIcon;

    private IDisposable buttonListener;
    private bool continueRequested;

    private void Awake()
    {
        if (titleGroup != null)
            titleGroup.alpha = 0f;
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
        if (titleGroup == null || blackCover == null || loadingIcon == null ||
            !Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError("[BootStrapper] Assign the Canvas references and include MainMenu in Build Settings.", this);
            yield break;
        }

        float startedAt = Time.realtimeSinceStartup;
        yield return null; // Render the black Canvas before beginning the menu load.

        AsyncOperation menuLoad = SceneManager.LoadSceneAsync("MainMenu");
        menuLoad.allowSceneActivation = false;

        while (menuLoad.progress < 0.9f || Time.realtimeSinceStartup - startedAt < minimumBlackSeconds)
            yield return null;

        float elapsed = 0f;
        while (elapsed < titleFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, titleFadeSeconds));
            titleGroup.alpha = progress;
            blackCover.alpha = 1f - progress;
            loadingIcon.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }
        titleGroup.alpha = 1f;
        blackCover.alpha = 0f;
        loadingIcon.gameObject.SetActive(false);

        buttonListener = InputSystem.onAnyButtonPress.Call(_ => continueRequested = true);
        while (!continueRequested)
            yield return null;

        buttonListener.Dispose();
        buttonListener = null;
        menuLoad.allowSceneActivation = true;
    }

    private void OnDestroy()
    {
        buttonListener?.Dispose();
    }
}
