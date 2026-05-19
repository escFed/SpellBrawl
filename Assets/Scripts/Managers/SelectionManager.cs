using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public int p1SelectedIndex;
    public int aiSelectedIndex;
    public bool isVsAI = true;

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
}
