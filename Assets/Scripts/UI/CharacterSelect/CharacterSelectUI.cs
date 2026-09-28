using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject gridContainer;
    [SerializeField] private Image selectImage;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private Transform selectGrid;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private MenuManager menuManager;

    private PlayerSlot selectingSlot = PlayerSlot.PlayerOne;
    private bool buttonsCreated;

    private void Start()
    {
        CreateCharacterButtons();
    }

    public void BeginSelection()
    {
        CreateCharacterButtons();
        selectingSlot = PlayerSlot.PlayerOne;

        if (gridContainer != null)
            gridContainer.SetActive(true);

        if (cardsPanel != null)
            cardsPanel.SetActive(false);

        if (selectImage != null && selectImage.gameObject != gridContainer)
            selectImage.sprite = null;

        RefreshText();
    }

    private void CreateCharacterButtons()
    {
        if (buttonsCreated)
            return;

        if (SelectionManager.Instance == null || SelectionManager.Instance.characterDb == null)
        {
            Debug.LogError("[CharacterSelectUI] SelectionManager needs a CharacterDatabase.", this);
            return;
        }

        CharacterDatabase db = SelectionManager.Instance.characterDb;

        for (int i = 0; i < db.CharacterCount; i++)
        {
            GameObject button = Instantiate(characterButtonPrefab, selectGrid);
            CharacterStats stats = db.GetCharacter(i);

            button.GetComponent<CharacterSlotButton>().Setup(i, stats, this);
        }

        buttonsCreated = true;
    }

    public void ShowCharacterPreview(Sprite icon, string name, int index)
    {
        if (selectImage != null && selectImage.gameObject != gridContainer)
            selectImage.sprite = icon;

        SelectionManager selection = SelectionManager.Instance;
        if (selection == null)
            return;

        selection.SetSelectedCharacter(selectingSlot, index);

        if (selection.isTrainingMode)
        {
            if (menuManager != null)
                menuManager.GoToTrainingRoom();
            return;
        }

        if (selectingSlot == PlayerSlot.PlayerOne)
        {
            selectingSlot = PlayerSlot.PlayerTwo;
            RefreshText();
            UIFocus.SelectFirst(gridContainer);
            return;
        }

        if (menuManager != null)
            menuManager.HandleCharacterSelectionComplete();
    }

    public void ResetSelection()
    {
        // Limpia la imagen y el texto
        if (selectImage != null) selectImage.sprite = null;
        if (selectText != null) selectText.text = "";

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.p1SelectedIndex = -1;
            SelectionManager.Instance.p2SelectedIndex = -1;
        }

        selectingSlot = PlayerSlot.PlayerOne;
        RefreshText();
    }

    private void RefreshText()
    {
        if (selectText == null)
            return;

        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.isTrainingMode)
        {
            selectText.text = "SELECT YOUR CHARACTER";
            return;
        }

        string participant = selection.GetDisplayName(selectingSlot).ToUpperInvariant();
        selectText.text = $"SELECT {participant} CHARACTER";
    }
}
