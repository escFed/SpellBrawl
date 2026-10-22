using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class StageSelectionUI : MonoBehaviour
{
    [Header("Selection UI")]
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private UnityEngine.UI.Button map1Button;
    [SerializeField] private UnityEngine.UI.Button map2Button;
    [SerializeField] private UnityEngine.UI.Button randomMapButton;
    [SerializeField] private UnityEngine.UI.Button fightButton;
    [SerializeField] private string noSelectionDisplayName = "Select a stage";

    [Header("Transition")]
    [SerializeField] private CanvasGroup blackTransition;
    [SerializeField, Min(0f)] private float transitionSeconds = 0.4f;

    [Header("Scenes")]
    [SerializeField] private string map1SceneName = "Stage1";
    [SerializeField] private string map2SceneName = "Stage2";

    [Header("Display Names")]
    [SerializeField] private string map1DisplayName = "Stage 1";
    [SerializeField] private string map2DisplayName = "Stage 2";
    [SerializeField] private string randomDisplayName = "Random";

    private MapPool selectedMap = MapPool.Stage1;
    private bool hasSelectedMap;
    private bool transitionStarted;

    private void OnEnable()
    {
        transitionStarted = false;
        ResetSelection();
    }

    private void LateUpdate()
    {
        if (!hasSelectedMap)
            RefreshSelection();
    }

    public void SelectMap1()
    {
        SetSelection(MapPool.Stage1);
    }

    public void SelectMap2()
    {
        SetSelection(MapPool.Stage2);
    }

    public void SelectRandomMap()
    {
        SetSelection(MapPool.Random);
    }

    public bool TryReturnToMapSelection()
    {
        if (!hasSelectedMap)
            return false;

        if (transitionStarted)
            return true;

        UnityEngine.UI.Button previousMapButton = selectedMap switch
        {
            MapPool.Stage1 => map1Button,
            MapPool.Stage2 => map2Button,
            MapPool.Random => randomMapButton,
            _ => null
        };

        ResetSelection();
        if (previousMapButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousMapButton.gameObject);
        return true;
    }

    public void StartMatch()
    {
        if (transitionStarted || !hasSelectedMap)
            return;

        string sceneName = ResolveSelectedSceneName();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[StageSelectionUI] The selected map has no scene name assigned.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[StageSelectionUI] Scene '{sceneName}' cannot be loaded. Add it to Build Settings before starting the match.",
                this);
            return;
        }

        if (blackTransition == null)
        {
            Debug.LogError("[StageSelectionUI] Assign the black transition CanvasGroup.", this);
            return;
        }

        transitionStarted = true;
        StartCoroutine(EnterLoadingScreen(sceneName));
    }

    private IEnumerator EnterLoadingScreen(string sceneName)
    {
        blackTransition.gameObject.SetActive(true);
        blackTransition.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            blackTransition.alpha = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, transitionSeconds));
            yield return null;
        }
        blackTransition.alpha = 1f;
        LoadingScreen.LoadStage(sceneName);
    }

    private void SetSelection(MapPool choice)
    {
        if (hasSelectedMap)
            return;

        if (fightButton == null)
        {
            Debug.LogError("[StageSelectionUI] Assign the Fight button.", this);
            return;
        }

        selectedMap = choice;
        hasSelectedMap = true;
        RefreshSelection();
        SetMapButtonsInteractable(false);

        fightButton.interactable = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(fightButton.gameObject);
    }

    private void SetMapButtonsInteractable(bool interactable)
    {
        if (map1Button != null) map1Button.interactable = interactable;
        if (map2Button != null) map2Button.interactable = interactable;
        if (randomMapButton != null) randomMapButton.interactable = interactable;
    }

    private void ResetSelection()
    {
        hasSelectedMap = false;
        SetMapButtonsInteractable(true);
        if (fightButton != null)
            fightButton.interactable = false;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (mapNameText == null)
            return;

        string displayName;
        if (hasSelectedMap)
        {
            displayName = GetMapDisplayName(selectedMap);
        }
        else
        {
            GameObject focusedObject = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            if (map1Button != null && map1Button.IsInteractable() && focusedObject == map1Button.gameObject)
                displayName = map1DisplayName;
            else if (map2Button != null && map2Button.IsInteractable() && focusedObject == map2Button.gameObject)
                displayName = map2DisplayName;
            else if (randomMapButton != null && randomMapButton.IsInteractable() && focusedObject == randomMapButton.gameObject)
                displayName = randomDisplayName;
            else
                displayName = noSelectionDisplayName;
        }

        if (mapNameText.text != displayName)
            mapNameText.text = displayName;
    }

    private string GetMapDisplayName(MapPool map)
    {
        return map switch
        {
            MapPool.Stage1 => map1DisplayName,
            MapPool.Stage2 => map2DisplayName,
            MapPool.Random => randomDisplayName,
            _ => noSelectionDisplayName
        };
    }

    private string ResolveSelectedSceneName()
    {
        switch (selectedMap)
        {
            case MapPool.Stage1:
                return map1SceneName;
            case MapPool.Stage2:
                return map2SceneName;
            case MapPool.Random:
                return Random.Range(0, 2) == 0 ? map1SceneName : map2SceneName;
            default:
                return string.Empty;
        }
    }

}
