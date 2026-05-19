using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Transform selectGrid;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Button nextButton;

    private void Start()
    { 
        if (nextButton != null) nextButton.interactable = false;

        CharacterDatabase db = SelectionManager.Instance.characterDb;

        for (int i = 0; i < db.CharacterCount; i++)
        {
            GameObject newButton = Instantiate(characterButtonPrefab, selectGrid);
            CharacterStats stats = db.GetCharacter(i);

            newButton.GetComponent<CharacterSlotButton>().Setup(i, stats, this);
        }
    }

    public void EnableNextButton()
    {
        if (nextButton != null) nextButton.interactable = true;
    }
}