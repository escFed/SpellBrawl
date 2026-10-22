using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UICard : Selectable, ISubmitHandler
{

 

    [Header("References")]
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;
    
    [Header("Card Visuals Mapping")]
    [SerializeField] private Image cardVisualPrefabInstance;
    [SerializeField] private Image cardPrevImage;

    private CanvasGroup selectedFade;

   
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


    public void UpdateVisuals()
    {
        if (cardPrefab != null && deckBuilder != null)
        {


            int count = deckBuilder.GetCardCount(cardPrefab);


            if (cardCopiesText != null)
                cardCopiesText.text = count > 0 ? "SELECTED" : string.Empty;

            // Keep selected roster cards usable for X/info while making the choice visible.
            if (IsInteractable())
            {
                if (selectedFade == null)
                {
                    selectedFade = GetComponent<CanvasGroup>();
                    if (selectedFade == null)
                        selectedFade = gameObject.AddComponent<CanvasGroup>();
                }

                selectedFade.alpha = count > 0 ? 0.5f : 1f;
            }
        }
    }

    public void Setup(GameObject prefab, ICardable cardData, DeckBuilderUI builder)
    {
        cardPrefab = prefab;
        deckBuilder = builder;
        UpdateVisuals();
    }
}
