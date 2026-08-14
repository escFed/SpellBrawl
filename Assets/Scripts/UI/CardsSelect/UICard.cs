using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UICard : Selectable, ISubmitHandler, ICancelHandler
{
    [Header("References")]
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;

    protected override void Awake()
    {
        base.Awake();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

    protected override void Start()
    {
        base.Start();
        UpdateVisuals();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (deckBuilder != null && deckBuilder.AddCardToDeck(cardPrefab))
            UpdateVisuals();
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (deckBuilder != null && deckBuilder.TryRemoveCard(cardPrefab))
            UpdateVisuals();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        ShowDescription();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (deckBuilder != null)
            deckBuilder.HideCardDescription();
    }

    public void UpdateVisuals()
    {
        int count = deckBuilder.GetCardCount(cardPrefab);
        if (cardCopiesText != null) cardCopiesText.text = count.ToString() + "/5";
    }

    private void ShowDescription()
    {
        if (deckBuilder == null || cardPrefab == null)
            return;

        ICardable cardData = cardPrefab.GetComponent<ICardable>();
        if (cardData != null)
        {
            deckBuilder.ShowCardDescription(cardData.CardName, cardData.CardDescription, cardData.EnergyCost, cardData.DamageableOrNot, cardData.Type);
        }
    }
}
