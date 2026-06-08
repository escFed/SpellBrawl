using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Deck Settings")]
    public int DeckSize = 20;
    public int CopiesPerCard = 5;

    [Header("UI Settings")]
    public TextMeshProUGUI DeckSizeText;
    public Button startMatchButton;

    [Header("Catalog")]
    public List<GameObject> allAvailableCards = new List<GameObject>();

    private Dictionary<GameObject, int> deckCounts = new Dictionary<GameObject, int>();
    private int currentTotalCards = 0;

    public void Start()
    {
        UpdateUI();
    }

    public bool AddCardToDeck(GameObject cardPrefab)
    {
        if (currentTotalCards >= DeckSize) return false;
        if (!deckCounts.ContainsKey(cardPrefab)) deckCounts[cardPrefab] = 0;
        if (deckCounts[cardPrefab] >= CopiesPerCard) return false;

        deckCounts[cardPrefab]++;
        currentTotalCards++;
        UpdateUI();
        return true;
    }

    public void RemoveCard(GameObject cardPrefab)
    {
        if (deckCounts.ContainsKey(cardPrefab) && deckCounts[cardPrefab] > 0)
        {
            deckCounts[cardPrefab]--;
            currentTotalCards--;
            UpdateUI();
        }
    }

    public int GetCardCount(GameObject cardPrefab)
    {
        if (deckCounts.ContainsKey(cardPrefab)) return deckCounts[cardPrefab];
        return 0;
    }

    private void UpdateUI()
    {
        if (DeckSizeText != null)
            DeckSizeText.text = currentTotalCards + " / " + DeckSize;

        if (startMatchButton != null)
            startMatchButton.interactable = (currentTotalCards == DeckSize);
    }

    public void ClearDeck()
    {
        deckCounts.Clear();
        currentTotalCards = 0;
        UpdateUI();
        RefreshAllUICards();
    }

    public void RandomizeDeck()
    {
        ClearDeck();

        if (allAvailableCards == null || allAvailableCards.Count == 0) return;

        int count = 0;

        while (currentTotalCards < DeckSize && count < 1000)
        {
            int randomIndex = Random.Range(0, allAvailableCards.Count);
            AddCardToDeck(allAvailableCards[randomIndex]);
            count++;
        }

        RefreshAllUICards();
    }

    private void RefreshAllUICards()
    {
        UICard[] allCards = FindObjectsByType<UICard>(FindObjectsSortMode.None);
        foreach (UICard card in allCards)
        {
            card.UpdateVisuals();
        }
    }

    public void SaveDeck()
    {
        List<GameObject> finalDeck = new List<GameObject>();
        foreach (var kvp in deckCounts)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                finalDeck.Add(kvp.Key);
            }
        }

        for (int i = 0; i < finalDeck.Count; i++)
        {
            GameObject temp = finalDeck[i];
            int randomIndex = Random.Range(i, finalDeck.Count);
            finalDeck[i] = finalDeck[randomIndex];
            finalDeck[randomIndex] = temp;
        }

        DeckManager.Instance.characterDeck = finalDeck;

        SceneManager.LoadScene("Stage1");
    }
}
