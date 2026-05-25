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
    }

    public void OnSelectCharacter()
    {
        SelectionManager.Instance.p1SelectedIndex = characterIndex;

        int totalCharacters = SelectionManager.Instance.characterDb.CharacterCount;
        SelectionManager.Instance.aiSelectedIndex = Random.Range(0, totalCharacters);

        uiManager.ShowCharacterPreview(characterIcon.sprite, characterName.text);
    }
}
