using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Deck Configuration")]
    [SerializeField] private DeckRules rules;
    [SerializeField] private CardCatalog catalog;

    [Header("Selected Cards UI")]
    [SerializeField] private Transform selectedCardsPanel;
    [SerializeField] private GameObject cardVisualPrefab;

    private readonly Dictionary<GameObject, GameObject> cardVisuals = new Dictionary<GameObject, GameObject>();

    [Header("UI Settings")]
   
    public Button startMatchButton;

    [Header("Card Settings")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitleText;
    public TextMeshProUGUI tooltipDescText;
    public TextMeshProUGUI costText;
   // public TextMeshProUGUI damageInfo;
    public TextMeshProUGUI cardTypeText;

    private readonly List<GameObject> selectedCards = new List<GameObject>();
    private readonly HashSet<GameObject> selectedCardSet = new HashSet<GameObject>();

    [Header("UI Audio")]

    private AudioSource source;
    [SerializeField] private AudioClip aCardSelectedClip;

    public void Start()
    {
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
        if (source != null)
            GameSettings.RegisterSource(source, GameSound.SoundEffects);

        UpdateUI();
    }

    public bool AddCardToDeck(GameObject cardPrefab)
    {
        if (rules == null || cardPrefab == null || selectedCards.Count >= rules.DeckSize)
            return false;

        if (catalog == null || !catalog.Contains(cardPrefab) || !selectedCardSet.Add(cardPrefab))
            return false;

        selectedCards.Add(cardPrefab);


        if (selectedCardsPanel != null && cardVisualPrefab != null)
        {
            cardVisualPrefab.SetActive(true);
            GameObject cardVisual = Instantiate(cardVisualPrefab, selectedCardsPanel);
                UICard ui = cardVisual.GetComponent<UICard>();
            if (ui != null)
            {
ui.cardPrefab = cardPrefab;
ui.deckBuilder = this;
                ui.UpdateVisuals();

            }


            // Guardar referencia
            cardVisuals[cardPrefab] = cardVisual;

            if (source != null && aCardSelectedClip != null)
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
      

        if(cardVisuals.TryGetValue(cardPrefab, out GameObject cardVisual))
        {
            Destroy(cardVisual);
            cardVisuals.Remove(cardPrefab);
        }
        UpdateUI();
        return true;
    }

    public int GetCardCount(GameObject cardPrefab)
    {
        return cardPrefab != null && selectedCardSet.Contains(cardPrefab) ? 1 : 0;
    }

    private void UpdateUI()
    {
      

        if (startMatchButton != null)
            startMatchButton.interactable = rules != null && selectedCards.Count == rules.DeckSize;
    }

    public void ClearDeck()
    {
        selectedCards.Clear();
        selectedCardSet.Clear();

        foreach(var kvp in cardVisuals)
        {
            if(kvp.Value != null)
                Destroy(kvp.Value);
        }
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
                Destroy(kvp.Value);
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

            selectedCardSet.Add(cardPrefab);
            if (selectedCardsPanel != null && cardVisualPrefab != null)
            {
               
                GameObject cardVisual = Instantiate(cardVisualPrefab, selectedCardsPanel);
                cardVisual.SetActive(true);
                UICard ui = cardVisual.GetComponent<UICard>();
                if (ui != null)
                {
                    ui.cardPrefab = cardPrefab;
                    ui.deckBuilder = this;
                    ui.UpdateVisuals();
                }
                // Guardar referencia
                cardVisuals[cardPrefab] = cardVisual;
            }

           
        }

        UpdateUI();
        
    }

    private void RefreshAllUICards()
    {
        UICard[] allCards = FindObjectsByType<UICard>(FindObjectsSortMode.None);
        foreach (UICard card in allCards)
        {
            card.UpdateVisuals();
        }
    }

    public void ShowCardDescription(string cardName, string description, int cost, string damage, CardType type)
    {
        if (tooltipTitleText != null) tooltipTitleText.text = cardName;
        if (tooltipDescText != null) tooltipDescText.text = description;
        if (cost > 0 && costText != null) costText.text = cost.ToString();
       // if (damageInfo != null) damageInfo.text = damage;
        if (cardTypeText != null) cardTypeText.text = type.ToString();
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void HideCardDescription()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    public void SaveDeck()
    {
        if (rules == null || DeckManager.Instance == null ||
            !DeckManager.Instance.TrySetDeck(selectedCards, rules.DeckSize))
        {
            Debug.LogError("[DeckBuilderUI] Cannot start the match with an invalid deck.", this);
            return;
        }

        SceneManager.LoadScene("Stage1");
    }
}

