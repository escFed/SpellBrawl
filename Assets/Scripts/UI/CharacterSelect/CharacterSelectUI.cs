using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject gridContainer;
    [SerializeField] private GameObject confirmGroup;
    [SerializeField] private Image selectImage;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private Transform selectGrid;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Button nextButton;

    private void Start()
    {
        if (gridContainer != null) gridContainer.SetActive(true);
        if (confirmGroup != null) confirmGroup.SetActive(false);

        if (nextButton != null) nextButton.interactable = false;

        CharacterDatabase db = SelectionManager.Instance.characterDb;

        for (int i = 0; i < db.CharacterCount; i++)
        {
            GameObject button = Instantiate(characterButtonPrefab, selectGrid);
            CharacterStats stats = db.GetCharacter(i);

            button.GetComponent<CharacterSlotButton>().Setup(i, stats, this);
        }
    }

    public void ShowCharacterPreview(Sprite icon, string name)
    {
        if (selectImage != null) selectImage.sprite = icon;
        if (selectText != null) selectText.text = name;

        if (gridContainer != null) gridContainer.SetActive(false);
        if (confirmGroup != null) confirmGroup.SetActive(true);

        EnableNextButton();
    }

    public void UndoSelection()
    {
        if (gridContainer != null) gridContainer.SetActive(true);
        if (confirmGroup != null) confirmGroup.SetActive(false); 

        SelectionManager.Instance.p1SelectedIndex = -1;
        if (nextButton != null) nextButton.interactable = false;
    }

    public void EnableNextButton()
    {
        if (nextButton != null) nextButton.interactable = true;
    }
}