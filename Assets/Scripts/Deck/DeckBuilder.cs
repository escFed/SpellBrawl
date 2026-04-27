using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeckBuilder : MonoBehaviour
{
    [Header ("Deck Settings")]
    public int DeckSize = 20;
    public int CopiesPerCard = 5;

    [Header("UI Settings")]
    public TextMeshProUGUI DeckSizeText;
    public Button startMatchButton;

    private Dictionary<GameObject, int> deckCounts = new Dictionary<GameObject, int>();
    private int currentTotalCards = 0;

    public void Start()
    {
        UpdateUI();
    }

    public bool AddCardToDeck(GameObject cardPrefab)
    {
        if (currentTotalCards >= DeckSize) return false;

        if(!deckCounts.ContainsKey(cardPrefab)) deckCounts[cardPrefab] = 0;

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

    private void UpdateUI()
    {
        if (DeckSizeText != null)
            DeckSizeText.text = currentTotalCards + " / " + DeckSize;

        if (startMatchButton != null)
            startMatchButton.interactable = (currentTotalCards == DeckSize);
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
