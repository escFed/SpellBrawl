using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlotButton : MonoBehaviour
{
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    private int characterIndex;
    private CharacterSelectUI uiManager;

    public void Setup(int index, CharacterStats stats, CharacterSelectUI manager)
    {
        characterIndex = index;
        characterIcon.sprite = stats.characterIcon;
        characterName.text = stats.characterName;
        uiManager = manager;

        // Conectar el botón al método de selección
        GetComponent<Button>().onClick.AddListener(OnSelectCharacter);
    }

    private void OnSelectCharacter()
    {
        // Guardar el personaje elegido
        SelectionManager.Instance.p1SelectedIndex = characterIndex;

        // Asignar personaje aleatorio a la IA
        int totalCharacters = SelectionManager.Instance.characterDb.CharacterCount;
        SelectionManager.Instance.aiSelectedIndex = Random.Range(0, totalCharacters);

        // Mostrar preview y saltar al panel de cartas
        uiManager.ShowCharacterPreview(characterIcon.sprite, characterName.text, characterIndex);
    }
}
