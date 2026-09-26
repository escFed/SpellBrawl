using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public MatchMode matchMode = MatchMode.PlayerVsAI;
    public int p1SelectedIndex;
    public int p2SelectedIndex = 1;
    [HideInInspector]
    public bool isVsAI = true;
    public bool isTrainingMode;

    public CharacterDatabase characterDb;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void BeginMatchSetup(MatchMode mode)
    {
        matchMode = mode;
        isVsAI = mode == MatchMode.PlayerVsAI;
        isTrainingMode = false;
        p1SelectedIndex = -1;
        p2SelectedIndex = -1;
    }

    public void SetSelectedCharacter(PlayerSlot slot, int characterIndex)
    {
        if (slot == PlayerSlot.PlayerOne)
            p1SelectedIndex = characterIndex;
        else
            p2SelectedIndex = characterIndex;
    }

    public int GetSelectedCharacter(PlayerSlot slot)
    {
        return slot == PlayerSlot.PlayerOne ? p1SelectedIndex : p2SelectedIndex;
    }

    public PlayerMode GetControlMode(PlayerSlot slot)
    {
        return ModeRules.GetControlMode(matchMode, slot);
    }

    public string GetDisplayName(PlayerSlot slot)
    {
        return ModeRules.GetDisplayName(matchMode, slot);
    }
}
