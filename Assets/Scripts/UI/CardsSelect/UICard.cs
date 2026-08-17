using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UICard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;

    private ICardable cardData;
    private RectTransform rectTransform;


    private void Awake()
    {
        cardData = cardPrefab.GetComponent<ICardable>();
        rectTransform = GetComponent<RectTransform>();

        if (deckBuilder == null)
            deckBuilder = FindAnyObjectByType<DeckBuilderUI>();
    }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        ICardable cardData = cardPrefab.GetComponent<ICardable>();
        
        if (cardData != null) deckBuilder.ShowCardDescription(cardData.CardName, cardData.CardDescription, cardData.EnergyCost, cardData.DamageableOrNot, cardData.Type);
        // Animación de hover
        LeanTween.scale(rectTransform, Vector3.one * 2f, 0.05f).setEaseOutQuad();

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //deckBuilder.HideCardDescription();
        LeanTween.scale(rectTransform, Vector3.one, 0.1f).setEaseOutQuad();
    }

    public void UpdateVisuals()
    {
        int count = deckBuilder.GetCardCount(cardPrefab);
        if (cardCopiesText != null) cardCopiesText.text = count.ToString() + "/5";
    }
}
