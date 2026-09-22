using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class StageSelectionUI : MonoBehaviour
{
    [Header("Selection UI")]
    [SerializeField] private TextMeshProUGUI mapNameText;

    [Header("Scenes")]
    [SerializeField] private string map1SceneName = "Stage1";
    [SerializeField] private string map2SceneName = "Stage2";

    [Header("Display Names")]
    [SerializeField] private string map1DisplayName = "Stage 1";
    [SerializeField] private string map2DisplayName = "Stage 2";
    [SerializeField] private string randomDisplayName = "Random";

    private MapPool selectedMap = MapPool.Stage1;

    private void OnEnable()
    {
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

    public void StartMatch()
    {
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

        SceneManager.LoadScene(sceneName);
    }

    private void SetSelection(MapPool choice)
    {
        selectedMap = choice;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (mapNameText == null)
            return;

        switch (selectedMap)
        {
            case MapPool.Stage1:
                mapNameText.text = map1DisplayName;
                break;
            case MapPool.Stage2:
                mapNameText.text = map2DisplayName;
                break;
            case MapPool.Random:
                mapNameText.text = randomDisplayName;
                break;
        }
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
