using System.Collections.Generic;
using UnityEngine;

public class CharacterDeck : MonoBehaviour
{
    [Header("Cards")]
    public GameObject[] cardPrefabsPool;
    public int totalDeckSize = 20;

    private List<ICardable> reserveDeck = new List<ICardable>();
    private ICardable[] currentHand = new ICardable[5];

    private PlayerController controller;
    private EnergyManager energy;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyManager>();
    }

    private void Start()
    {
        if (DeckManager.Instance != null && DeckManager.Instance.characterDeck.Count > 0 && controller.PlayerIndex == 0)
        {
            cardPrefabsPool = DeckManager.Instance.characterDeck.ToArray();
        }

        ResetDeckForNewRound();
    }

    public ICardable[] GetCurrentHand() => currentHand;

    public void ResetDeckForNewRound()
    {
        foreach (var card in currentHand) { if (card != null && card is MonoBehaviour mb) Destroy(mb.gameObject); }
        foreach (var card in reserveDeck) { if (card != null && card is MonoBehaviour mb) Destroy(mb.gameObject); }

        reserveDeck.Clear();
        for (int i = 0; i < currentHand.Length; i++) currentHand[i] = null;

        if (cardPrefabsPool != null && cardPrefabsPool.Length > 0)
        {
            for (int i = 0; i < totalDeckSize; i++)
            {
                GameObject randomPrefab = cardPrefabsPool[Random.Range(0, cardPrefabsPool.Length)];
                GameObject newCard = Instantiate(randomPrefab, transform.position, Quaternion.identity, transform);
                newCard.SetActive(false);
                reserveDeck.Add(newCard.GetComponent<ICardable>());
            }
        }

        DrawCardsToFillHand();
        UpdateHandUI();
        UpdateDeckCountUI();
    }

    public void TryUseCardFromHand(int handIndex)
    {
        if (handIndex < currentHand.Length && currentHand[handIndex] != null)
        {
            ICardable cardToUse = currentHand[handIndex];

            if (!cardToUse.CanBeUsed(controller))
            {
                Debug.Log("U cant use this card");
                return;
            }
            if (energy != null && !energy.TrySpendEnergy(cardToUse.EnergyCost))
            {
                return;
            }
            if (cardToUse is MonoBehaviour mb)
            {
                mb.gameObject.SetActive(true);
            }

            controller.ExecuteCardState(cardToUse);

            currentHand[handIndex] = null;
            UpdateHandUI();
        }
    }

    public void TryDrawNewHand()
    {
        if (energy == null || !energy.TrySpendEnergy(40)) return;

        for (int i = 0; i < currentHand.Length; i++)
        {
            if (currentHand[i] != null)
            {
                if (currentHand[i] is MonoBehaviour mb) Destroy(mb.gameObject);
                currentHand[i] = null;
            }
        }

        DrawCardsToFillHand();
        UpdateHandUI();
        UpdateDeckCountUI();
    }

    private void DrawCardsToFillHand()
    {
        for (int i = 0; i < 5; i++)
        {
            if (reserveDeck.Count > 0)
            {
                currentHand[i] = reserveDeck[0];
                reserveDeck.RemoveAt(0);
            }
        }
    }

    public void ForceSabotageRedraw()
    {
        for (int i = 0; i < currentHand.Length; i++)
        {
            if (currentHand[i] != null)
            {
                if (currentHand[i] is MonoBehaviour mb) Destroy(mb.gameObject);
                currentHand[i] = null;
            }
        }

        DrawCardsToFillHand();
        UpdateHandUI();
        UpdateDeckCountUI();
    }

    public void UpdateHandUI()
    {
        if (controller != null) UIEvents.OnHandChanged?.Invoke(controller.PlayerIndex, currentHand);
    }

    public void UpdateDeckCountUI()
    {
        if (controller != null) UIEvents.OnDeckCountChanged?.Invoke(controller.PlayerIndex, reserveDeck.Count);
    }
}
