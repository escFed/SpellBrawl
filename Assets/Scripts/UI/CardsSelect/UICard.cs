using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UICard : MonoBehaviour, IPointerClickHandler
{
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;

    private void Start() => UpdateVisuals();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (deckBuilder.AddCardToDeck(cardPrefab))
            {
                UpdateVisuals();
            }
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            deckBuilder.RemoveCard(cardPrefab);
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        int count = deckBuilder.GetCardCount(cardPrefab);
        if (cardCopiesText != null)
            cardCopiesText.text = count.ToString() + "/5";
    }
}
