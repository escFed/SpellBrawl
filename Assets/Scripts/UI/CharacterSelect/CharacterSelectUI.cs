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

    private void Start()
    {
        if (gridContainer != null) gridContainer.SetActive(true);

        CharacterDatabase db = SelectionManager.Instance.characterDb;

        for (int i = 0; i < db.CharacterCount; i++)
        {
            GameObject button = Instantiate(characterButtonPrefab, selectGrid);
            CharacterStats stats = db.GetCharacter(i);

            button.GetComponent<CharacterSlotButton>().Setup(i, stats, this);
        }
    }

    public void ShowCharacterPreview(Sprite icon, string name, int index)
    {
        if (selectImage != null) selectImage.sprite = icon;
        if (selectText != null) selectText.text = name;

        // Guardar el índice real del personaje
        SelectionManager.Instance.p1SelectedIndex = index;

        // Opción 1: cambiar de panel dentro de la misma escena
        if (cardsPanel != null)
        {
            gridContainer.SetActive(false);
            cardsPanel.SetActive(true);
        }


    }

    public void ResetSelection()
    {
        // Limpia la imagen y el texto
        if (selectImage != null) selectImage.sprite = null;
        if (selectText != null) selectText.text = "";

        // Resetea el índice en el SelectionManager
        SelectionManager.Instance.p1SelectedIndex = -1;
    }

}
