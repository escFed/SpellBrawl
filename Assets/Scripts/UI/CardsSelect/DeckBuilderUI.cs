using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Deck Configuration")]
    [SerializeField] private DeckRules rules;
    [SerializeField] private CardCatalog catalog;

    [Header("Selected Cards UI")]
    [SerializeField] public Transform selectedCardsPanel;
    [SerializeField] private GameObject cardVisualPrefab;
    [SerializeField] private TextMeshProUGUI deckSizeText;
    [SerializeField] private TextMeshProUGUI selectingPlayerText;
    private Dictionary<GameObject, GameObject> cardVisuals = new Dictionary<GameObject, GameObject>();

    [Header("UI Settings")]
    [SerializeField] private MenuManager menuManager;
    public Button startMatchButton;

    [Header("Card Settings")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitleText;
    public TextMeshProUGUI tooltipDescText;
    public TextMeshProUGUI costText;
    public Sprite genericCard;
    public TextMeshProUGUI cardTypeText;

    [Header("Card Info Overlay")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image infoCardImage;
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI infoDescriptionText;
    [SerializeField] private RectTransform cardActionHints;
    [SerializeField, Min(0f)] private float infoZoomDuration = 0.25f;

    private CanvasGroup infoCanvasGroup;
    private RectTransform infoCardRect;
    private Vector3 infoCardPosition;
    private Vector3 infoCardScale;
    private UICard infoSourceCard;
    private Coroutine infoAnimation;
    private InputAction infoAction;
    private readonly Vector3[] cardCorners = new Vector3[4];

    public bool IsCardInfoOpen => infoPanel != null && infoPanel.activeSelf;

    private List<GameObject> selectedCards = new List<GameObject>();
    private HashSet<GameObject> selectedCardSet = new HashSet<GameObject>();
    private PlayerSlot selectingSlot = PlayerSlot.PlayerOne;
    private bool initialized;

    public PlayerSlot SelectingSlot => selectingSlot;
    private readonly List<Selectable> fightNavigationSources = new List<Selectable>();
    private readonly List<Navigation> fightNavigationLinks = new List<Navigation>();

    [Header("UI Audio")]
    private AudioSource source;
    [SerializeField] private AudioClip aCardSelectedClip;

    public void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        infoAction = InputSystem.actions?.FindAction("UI/CardInfo");
    }

    private void OnDisable()
    {
        CloseCardInfo(false);
        if (cardActionHints != null)
            cardActionHints.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (initialized && !IsCardInfoOpen && infoAction != null && infoAction.WasPressedThisFrame() &&
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.TryGetComponent(out UICard card))
            OpenCardInfo(card);
    }

    private void LateUpdate()
    {
        if (cardActionHints == null)
            return;

        GameObject focused = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        UICard card = focused != null ? focused.GetComponent<UICard>() : null;
        bool show = !IsCardInfoOpen && card != null && card.deckBuilder == this &&
            card.cardPrefab != null && card.IsActive() && card.IsInteractable();
        if (cardActionHints.gameObject.activeSelf != show)
            cardActionHints.gameObject.SetActive(show);
        if (!show)
            return;

        RectTransform parent = cardActionHints.parent as RectTransform;
        RectTransform cardRect = card.transform as RectTransform;
        if (parent == null || cardRect == null)
            return;

        cardRect.GetWorldCorners(cardCorners);
        Vector3 bottomCenter = (cardCorners[0] + cardCorners[3]) * 0.5f;
        Vector3 localPosition = parent.InverseTransformPoint(bottomCenter);
        cardActionHints.anchoredPosition = new Vector2(localPosition.x, localPosition.y - 28f);
    }

    public void BeginSelection(PlayerSlot slot)
    {
        Initialize();
        if (!initialized)
            return;

        CloseCardInfo(false);
        selectingSlot = slot;
        ClearDeck();
    }

    public bool TryReturnToPlayerOneDeck()
    {
        if (selectingSlot != PlayerSlot.PlayerTwo)
            return false;

        BeginSelection(PlayerSlot.PlayerOne);
        IReadOnlyList<GameObject> savedDeck = DeckManager.Instance?.GetDeck(PlayerSlot.PlayerOne);
        if (savedDeck != null)
        {
            for (int i = 0; i < savedDeck.Count; i++)
                AddCardToDeck(savedDeck[i], false);
        }

        RefreshAllUICards();
        return true;
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (rules == null)
        {
            Debug.LogError("[DeckBuilderUI] DeckRules is not assigned.", this);
            enabled = false;
            return;
        }

        List<GameObject> availableCards = new List<GameObject>();
        if (catalog == null || catalog.CopyValidUniqueCardsTo(availableCards) < rules.DeckSize)
        {
            Debug.LogError($"[DeckBuilderUI] CardCatalog must contain at least {rules.DeckSize} unique valid cards.", this);
            enabled = false;
            return;
        }

        source = GetComponent<AudioSource>();

        if (infoPanel != null && infoCardImage != null)
        {
            infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            infoCardRect = infoCardImage.rectTransform;
            infoCardPosition = infoCardRect.localPosition;
            infoCardScale = infoCardRect.localScale;
        }

        if (source != null)
            GameSettings.RegisterSource(source, GameSound.SoundEffects);

        CacheFightNavigation();
        UpdateUI();
        initialized = true;
    }

    public bool OpenCardInfo(UICard card)
    {
        if (IsCardInfoOpen || card == null || card.deckBuilder != this || card.cardPrefab == null ||
            infoPanel == null || infoCardImage == null || infoNameText == null ||
            infoDescriptionText == null || infoCanvasGroup == null || infoCardRect == null)
            return false;

        ICardable cardData = card.cardPrefab.GetComponent<ICardable>();
        if (cardData == null)
            return false;

        infoSourceCard = card;
        infoCardImage.sprite = cardData.CardVisual;
        infoNameText.text = cardData.CardName;
        infoDescriptionText.text = cardData.CardDescription;

        RectTransform sourceRect = card.transform as RectTransform;
        Bounds sourceBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(infoPanel.transform, sourceRect);
        float widthRatio = sourceBounds.size.x / Mathf.Max(1f, infoCardRect.rect.width);
        float heightRatio = sourceBounds.size.y / Mathf.Max(1f, infoCardRect.rect.height);
        float startScale = Mathf.Clamp(Mathf.Min(widthRatio, heightRatio), 0.05f, 1f);

        infoPanel.SetActive(true);
        infoCanvasGroup.alpha = 0f;
        infoCardRect.localPosition = sourceBounds.center;
        infoCardRect.localScale = infoCardScale * startScale;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        infoAnimation = StartCoroutine(AnimateCardInfo());
        return true;
    }

    public bool CloseCardInfo(bool restoreFocus = true)
    {
        if (!IsCardInfoOpen)
            return false;

        if (infoAnimation != null)
        {
            StopCoroutine(infoAnimation);
            infoAnimation = null;
        }

        infoPanel.SetActive(false);
        if (infoCanvasGroup != null)
            infoCanvasGroup.alpha = 1f;
        if (infoCardRect != null)
        {
            infoCardRect.localPosition = infoCardPosition;
            infoCardRect.localScale = infoCardScale;
        }

        if (restoreFocus && infoSourceCard != null && EventSystem.current != null &&
            infoSourceCard.IsActive() && infoSourceCard.IsInteractable())
            EventSystem.current.SetSelectedGameObject(infoSourceCard.gameObject);

        infoSourceCard = null;
        return true;
    }

    private IEnumerator AnimateCardInfo()
    {
        Vector3 startPosition = infoCardRect.localPosition;
        Vector3 startScale = infoCardRect.localScale;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, infoZoomDuration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            infoCardRect.localPosition = Vector3.LerpUnclamped(startPosition, infoCardPosition, t);
            infoCardRect.localScale = Vector3.LerpUnclamped(startScale, infoCardScale, t);
            infoCanvasGroup.alpha = t;
            yield return null;
        }

        infoCardRect.localPosition = infoCardPosition;
        infoCardRect.localScale = infoCardScale;
        infoCanvasGroup.alpha = 1f;
        infoAnimation = null;
    }

    public bool AddCardToDeck(GameObject cardPrefab)
    {
        return AddCardToDeck(cardPrefab, true);
    }

    private bool AddCardToDeck(GameObject cardPrefab, bool playSound)
    {
        if (rules == null || cardPrefab == null || selectedCards.Count >= rules.DeckSize)
            return false;

        if (catalog == null || !catalog.Contains(cardPrefab) || !selectedCardSet.Add(cardPrefab))
            return false;

        selectedCards.Add(cardPrefab);

        if (selectedCardsPanel != null && cardVisualPrefab != null)
        {
            GameObject cardVisual = Instantiate(cardVisualPrefab, selectedCardsPanel);
            cardVisual.SetActive(true);
            ConfigureSelectedCardVisual(cardVisual, cardPrefab, false);

            cardVisuals[cardPrefab] = cardVisual;
            RepositionCards();

            if (playSound && source != null && aCardSelectedClip != null)
                source.PlayOneShot(aCardSelectedClip);
        }
        
        UpdateUI();
        return true;
    }
    public bool TryRemoveCard(GameObject cardPrefab)
    {
        if (cardPrefab == null || !selectedCardSet.Remove(cardPrefab))
            return false;

        selectedCards.Remove(cardPrefab);

        if (cardVisuals.TryGetValue(cardPrefab, out GameObject cardVisual))
        {
            cardVisual.SetActive(false);
            Destroy(cardVisual);
            cardVisuals.Remove(cardPrefab);
            RepositionCards();
        }

        UpdateUI();
        return true;
    }

    public bool TryRemoveLastCard()
    {
        if (selectedCards.Count == 0)
            return false;

        GameObject lastCard = selectedCards[selectedCards.Count - 1];
        bool focusWillBeDestroyed = cardVisuals.TryGetValue(lastCard, out GameObject visual) &&
            EventSystem.current != null && EventSystem.current.currentSelectedGameObject == visual;
        if (!TryRemoveCard(lastCard))
            return false;

        RefreshAllUICards();
        if (focusWillBeDestroyed)
            UIFocus.SelectFirst(gameObject);
        return true;
    }

    public int GetCardCount(GameObject cardPrefab)
    {
        return cardPrefab != null && selectedCardSet.Contains(cardPrefab) ? 1 : 0;
    }

    private void UpdateUI()
    {
        if (deckSizeText != null)
            deckSizeText.text = rules != null ? $"{selectedCards.Count}/{rules.DeckSize}" : "0/0";

        if (selectingPlayerText != null)
            selectingPlayerText.text = GetParticipantLabel();

        if (startMatchButton != null)
        {
            bool canStart = rules != null && selectedCards.Count == rules.DeckSize;
            EventSystem eventSystem = EventSystem.current;
            bool fightWasSelected = !canStart && eventSystem != null &&
                eventSystem.currentSelectedGameObject == startMatchButton.gameObject;
            startMatchButton.interactable = canStart;
            SetFightNavigation(canStart);

            if (fightWasSelected)
            {
                Selectable previous = startMatchButton.navigation.selectOnUp;
                if (previous != null && previous.IsActive() && previous.IsInteractable())
                    eventSystem.SetSelectedGameObject(previous.gameObject);
                else
                    UIFocus.SelectFirst(gameObject);
            }
        }
    }

    private void CacheFightNavigation()
    {
        if (startMatchButton == null)
            return;

        // Explicit uGUI links can select a non-interactable button, so remember every incoming link.
        foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
        {
            Navigation navigation = selectable.navigation;
            if (navigation.mode != Navigation.Mode.Explicit || selectable == startMatchButton)
                continue;

            if (navigation.selectOnUp == startMatchButton ||
                navigation.selectOnDown == startMatchButton ||
                navigation.selectOnLeft == startMatchButton ||
                navigation.selectOnRight == startMatchButton)
            {
                fightNavigationSources.Add(selectable);
                fightNavigationLinks.Add(navigation);
            }
        }
    }

    private void SetFightNavigation(bool canStart)
    {
        for (int i = 0; i < fightNavigationSources.Count; i++)
        {
            Selectable selectable = fightNavigationSources[i];
            if (selectable == null)
                continue;

            Navigation navigation = selectable.navigation;
            Navigation original = fightNavigationLinks[i];
            Selectable target = canStart ? startMatchButton : null;
            if (original.selectOnUp == startMatchButton) navigation.selectOnUp = target;
            if (original.selectOnDown == startMatchButton) navigation.selectOnDown = target;
            if (original.selectOnLeft == startMatchButton) navigation.selectOnLeft = target;
            if (original.selectOnRight == startMatchButton) navigation.selectOnRight = target;

            selectable.navigation = navigation;
        }
    }

    public void ClearDeck()
    {
        selectedCards.Clear();
        selectedCardSet.Clear();

        foreach (var kvp in cardVisuals)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(false);
                Destroy(kvp.Value);
            }
        }
        cardVisuals.Clear();
        UpdateUI();
        RefreshAllUICards();
    }

    public void RandomizeDeck()
    {
        if (rules == null || catalog == null)
            return;

        selectedCards.Clear();
        selectedCardSet.Clear();

        foreach (var kvp in cardVisuals)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(false);
                Destroy(kvp.Value);
            }
        }
        cardVisuals.Clear();

        if (!DeckBuilder.TryBuild(null, catalog, rules.DeckSize, selectedCards, out int availableCardCount))
        {
            Debug.LogError($"[DeckBuilderUI] The catalog needs {rules.DeckSize} unique valid cards but found {availableCardCount}.", this);
            UpdateUI();
            RefreshAllUICards();
            return;
        }

        for (int i = 0; i < selectedCards.Count; i++)
            selectedCardSet.Add(selectedCards[i]);

        foreach (var cardPrefab in selectedCards)
        {
            if (selectedCardsPanel != null && cardVisualPrefab != null)
            {
                GameObject cardVisual = Instantiate(cardVisualPrefab, selectedCardsPanel);
                cardVisual.SetActive(true);
                ConfigureSelectedCardVisual(cardVisual, cardPrefab, true);
                cardVisuals[cardPrefab] = cardVisual;
            }
        }
        RepositionCards();
        UpdateUI();
        RefreshAllUICards();
    }

    private void ConfigureSelectedCardVisual(GameObject visual, GameObject cardPrefab, bool faceDown)
    {
        UICard ui = visual.GetComponent<UICard>();
        if (ui != null)
        {
            ui.cardPrefab = cardPrefab;
            ui.deckBuilder = this;
            ui.transition = Selectable.Transition.None;
            ui.UpdateVisuals();
        }

        Image image = visual.GetComponent<Image>();
        ICardable cardData = cardPrefab.GetComponent<ICardable>();
        Sprite cardSprite = faceDown ? genericCard : cardData?.CardVisual;
        if (image != null && cardSprite != null)
        {
            image.sprite = cardSprite;
            image.preserveAspect = true;
            image.color = Color.white;
        }
        else
        {
            Debug.LogError($"[DeckBuilderUI] Missing selected-card Image or sprite on {cardPrefab.name}.", visual);
        }
    }

    private void RefreshAllUICards()
    {
        UICard[] allCards = FindObjectsByType<UICard>(FindObjectsSortMode.None);
        foreach (UICard card in allCards)
        {
            card.UpdateVisuals();
        }
    }

    public void ShowCardDescription(string cardName, string description, string damage, CardType type)
    {
        if (tooltipTitleText != null) tooltipTitleText.text = cardName;
        if (tooltipDescText != null) tooltipDescText.text = description;
        if (costText != null) costText.gameObject.SetActive(false);
        if (cardTypeText != null) cardTypeText.text = type.ToString();
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void HideCardDescription()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    public void SaveDeck()
    {
        if (menuManager == null)
        {
            Debug.LogError("[DeckBuilderUI] Assign MenuManager in the Inspector before opening map selection.", this);
            return;
        }

        if (rules == null || DeckManager.Instance == null ||
            !DeckManager.Instance.TrySetDeck(selectingSlot, selectedCards, rules.DeckSize))
        {
            Debug.LogError("[DeckBuilderUI] Cannot start the match with an invalid deck.", this);
            return;
        }

        if (SelectionManager.Instance != null && selectingSlot == PlayerSlot.PlayerOne && ModeRules.UsesCustomDeck(SelectionManager.Instance.matchMode, PlayerSlot.PlayerTwo))
        {
            BeginSelection(PlayerSlot.PlayerTwo);
            UIFocus.SelectFirst(gameObject);
            return;
        }

        menuManager.ShowMapSelect();
    }

    private string GetParticipantLabel()
    {
        return selectingSlot == PlayerSlot.PlayerOne ? "P1" : "P2";
    }

    private void RepositionCards()
    {
        if (selectedCardsPanel is RectTransform panel)
            LayoutRebuilder.MarkLayoutForRebuild(panel);
    }
}
