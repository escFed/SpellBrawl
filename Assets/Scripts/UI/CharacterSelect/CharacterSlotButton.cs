using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlotButton : MonoBehaviour
{
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private AudioClip selectedClip;

    private AudioSource audioSource;
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
        uiManager.ShowCharacterPreview(characterIcon.sprite, characterName.text, characterIndex);
    }

    public void PlaySelectedSound()
    {
        audioSource = GetComponent<AudioSource>();
        GameSettings.RegisterSource(audioSource, GameSound.SoundEffects);
        audioSource.PlayOneShot(selectedClip);
    }
}
