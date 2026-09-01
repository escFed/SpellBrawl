using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent(typeof(Graphic))]
public class UICard : Selectable, ISubmitHandler, ICancelHandler
{

 

    [Header("References")]
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;
    
    [Header("Card Visuals Mapping")]
    [SerializeField] private Image cardVisualPrefabInstance;
    [SerializeField] private Image cardPrevImage;

   
    protected override void Awake()
    {
        base.Awake();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

    protected override void Start()
    {
        base.Start();
        if (cardVisualPrefabInstance != null)
        {


            cardVisualPrefabInstance.gameObject.SetActive(false);

        }
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
        ICardable cardData = cardPrefab.GetComponent<ICardable>();
        if (cardData != null)
        {
            // Actualizar panel de abajo
            if (cardVisualPrefabInstance != null)
            {
                cardVisualPrefabInstance.sprite = cardData.CardVisual;
                cardVisualPrefabInstance.gameObject.SetActive(true);
               
            }

            // Actualizar preview a la izquierda
            if (cardPrevImage != null)
            {
                cardPrevImage.sprite = cardData.CardVisual;
                cardPrevImage.gameObject.SetActive(true);
            }
        }
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
        if (cardPrefab != null && deckBuilder != null)
        {


            int count = deckBuilder.GetCardCount(cardPrefab);


            if (cardCopiesText != null)
                cardCopiesText.text = count > 0 ? "SELECTED" : string.Empty;
        }
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

    public void Setup(GameObject prefab, ICardable cardData, DeckBuilderUI builder)
    {
        cardPrefab = prefab;
        deckBuilder = builder;
        UpdateVisuals();
        deckBuilder.ShowCardDescription(cardData.CardName, cardData.CardDescription, cardData.EnergyCost, cardData.DamageableOrNot, cardData.Type);
    }
    public void ShowSpecificUICard()
    {
        if (deckBuilder != null && cardVisualPrefabInstance != null)
        {
            cardVisualPrefabInstance.gameObject.SetActive(true);

            ICardable cardData = cardPrefab.GetComponent<ICardable>();
            if (cardData != null)
            {
                Sprite imgSprite = cardData.CardVisual;
                if (imgSprite != null)
                {
                    cardVisualPrefabInstance.sprite = imgSprite;
                }

            }
        }
    }

}
