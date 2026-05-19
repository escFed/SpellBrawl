using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UICard : MonoBehaviour, IPointerClickHandler
{
    public GameObject cardPrefab;
    public DeckBuilder deckBuilder;
    public TextMeshProUGUI cardCopiesText;

    private int currentCopies = 0;

    private void Start() => UpdateVisuals();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (deckBuilder.AddCardToDeck(cardPrefab))
            {
                currentCopies++;
                UpdateVisuals();
            }
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentCopies > 0)
            {
                deckBuilder.RemoveCard(cardPrefab);
                currentCopies--;
                UpdateVisuals();
            }
        }
    }

    private void UpdateVisuals()
    {
        if (cardCopiesText != null)
            cardCopiesText.text = currentCopies.ToString() + "/5";
    }
}
