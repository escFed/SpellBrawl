using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class VersusIntroUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UnityEngine.UI.Image playerOnePortrait;
    [SerializeField] private UnityEngine.UI.Image playerTwoPortrait;
    [SerializeField] private TextMeshProUGUI playerOneName;
    [SerializeField] private TextMeshProUGUI playerTwoName;
    [SerializeField] private Material blueAmiroIconMaterial;

    public bool Show(SelectionManager selection)
    {
        if (selection == null || selection.characterDb == null ||
            canvasGroup == null || playerOnePortrait == null || playerTwoPortrait == null ||
            playerOneName == null || playerTwoName == null)
        {
            Debug.LogError("[VersusIntroUI] Assign the Canvas elements and CharacterDatabase.", this);
            return false;
        }

        CharacterDatabase roster = selection.characterDb;
        int firstIndex = selection.p1SelectedIndex;
        int secondIndex = selection.p2SelectedIndex;
        if (firstIndex < 0 || secondIndex < 0 ||
            firstIndex >= roster.CharacterCount || secondIndex >= roster.CharacterCount)
        {
            Debug.LogError("[VersusIntroUI] Both fighters must be selected before loading the match.", this);
            return false;
        }

        CharacterStats first = roster.GetCharacter(firstIndex);
        CharacterStats second = roster.GetCharacter(secondIndex);
        if (first == null || second == null)
            return false;

        playerOnePortrait.sprite = first.characterIcon;
        playerOnePortrait.enabled = first.characterIcon != null;
        playerTwoPortrait.sprite = second.characterIcon;
        playerTwoPortrait.enabled = second.characterIcon != null;

        Vector3 leftScale = playerOnePortrait.rectTransform.localScale;
        leftScale.x = Mathf.Abs(leftScale.x);
        playerOnePortrait.rectTransform.localScale = leftScale;
        Vector3 rightScale = playerTwoPortrait.rectTransform.localScale;
        rightScale.x = -Mathf.Abs(rightScale.x);
        playerTwoPortrait.rectTransform.localScale = rightScale;

        bool duplicateAmiro = first == second && second.characterPrefab != null &&
            second.characterPrefab.GetComponent<AmiroBlueVariant>() != null;
        playerTwoPortrait.material = duplicateAmiro ? blueAmiroIconMaterial : null;

        playerOneName.text = first.characterName.ToUpperInvariant();
        playerTwoName.text = second.characterName.ToUpperInvariant();
        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
        return true;
    }
}
