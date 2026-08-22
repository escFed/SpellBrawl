using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckRuntime
{
    private DeckRules rules;
    private List<GameObject> deckTemplate;
    private List<GameObject> drawStack;
    private List<GameObject> discardStack;
    private List<GameObject> redrawBuffer;
    private HandSlot[] handSlots;

    private int activeHandSize;
    private int fullDraws;

    public int HandSlotCount => handSlots.Length;
    public int DrawStackCount => drawStack.Count;
    public int FullDraws => fullDraws;
    public bool CanFullRedraw => fullDraws > 0;

    public DeckRuntime(DeckRules rules, IReadOnlyList<GameObject> deckTemplate)
    {
        this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        if (deckTemplate == null)
            throw new ArgumentNullException(nameof(deckTemplate));

        this.deckTemplate = new List<GameObject>(deckTemplate);
        drawStack = new List<GameObject>(deckTemplate.Count);
        discardStack = new List<GameObject>(deckTemplate.Count);
        redrawBuffer = new List<GameObject>(rules.MaxHandSize);
        handSlots = new HandSlot[rules.MaxHandSize];

        for (int i = 0; i < handSlots.Length; i++)
            handSlots[i] = new HandSlot();
    }

    public ICardable GetCardAt(int handIndex)
    {
        return IsValidSlot(handIndex) ? handSlots[handIndex].Card : null;
    }

    public bool IsSlotReady(int handIndex, float currentTime)
    {
        return IsValidSlot(handIndex) && handSlots[handIndex].IsReady(currentTime);
    }

    public CardActions GetUseStatus(int handIndex, float currentTime)
    {
        if (!IsValidSlot(handIndex))
            return CardActions.InvalidSlot;

        HandSlot slot = handSlots[handIndex];
        if (!slot.IsUnlocked)
            return CardActions.LockedSlot;
        if (!slot.HasCard)
            return CardActions.EmptySlot;
        if (!slot.IsReady(currentTime))
            return CardActions.CooldownActive;

        return CardActions.Success;
    }

    public bool ResetForNewRound()
    {
        drawStack.Clear();
        drawStack.AddRange(deckTemplate);
        discardStack.Clear();
        Shuffle(drawStack);

        activeHandSize = rules.InitialHandSize;
        fullDraws = rules.FullDraws;

        for (int i = 0; i < handSlots.Length; i++)
        {
            handSlots[i].Lock();
            if (i < activeHandSize)
                handSlots[i].Unlock();
        }

        bool filledInitialHand = true;
        for (int i = 0; i < activeHandSize; i++)
            filledInitialHand &= TryDrawIntoHandSlot(i, 0f);

        return filledInitialHand;
    }

    public GameObject ConsumeAndRefill(int handIndex, float replacementReadyAt)
    {
        HandSlot slot = handSlots[handIndex];
        GameObject usedPrefab = slot.CardPrefab;

        slot.Clear();
        discardStack.Add(usedPrefab);
        TryDrawIntoHandSlot(handIndex, replacementReadyAt);

        return usedPrefab;
    }

    public bool TryFullRedraw(float readyAt)
    {
        if (!CanFullRedraw || !TryReplaceActiveHand(readyAt))
            return false;

        fullDraws--;
        return true;
    }

    public bool TryForceRedraw(float readyAt)
    {
        return TryReplaceActiveHand(readyAt);
    }

    public bool TryUnlockNextSlot(float readyAt)
    {
        if (activeHandSize >= handSlots.Length || !TryDrawPrefab(out GameObject cardPrefab))
            return false;

        HandSlot slot = handSlots[activeHandSize];
        slot.Unlock();
        slot.Assign(cardPrefab, cardPrefab.GetComponent<ICardable>(), readyAt);
        activeHandSize++;
        return true;
    }

    public void CopyHandSnapshotTo(HandSlotView[] destination)
    {
        for (int i = 0; i < handSlots.Length; i++)
            destination[i] = handSlots[i].CreateView();
    }

    private bool TryDrawIntoHandSlot(int handIndex, float readyAt)
    {
        if (!TryDrawPrefab(out GameObject cardPrefab))
            return false;

        handSlots[handIndex].Assign(cardPrefab, cardPrefab.GetComponent<ICardable>(), readyAt);
        return true;
    }

    private bool TryReplaceActiveHand(float readyAt)
    {
        if (!CanDraw(activeHandSize))
            return false;

        redrawBuffer.Clear();
        for (int i = 0; i < activeHandSize; i++)
        {
            TryDrawPrefab(out GameObject cardPrefab);
            redrawBuffer.Add(cardPrefab);
        }

        for (int i = 0; i < activeHandSize; i++)
        {
            HandSlot slot = handSlots[i];
            discardStack.Add(slot.CardPrefab);

            GameObject replacementPrefab = redrawBuffer[i];
            slot.Assign(replacementPrefab, replacementPrefab.GetComponent<ICardable>(), readyAt);
        }

        return true;
    }

    private bool TryDrawPrefab(out GameObject cardPrefab)
    {
        if (drawStack.Count == 0)
            RecycleDiscardPile();

        if (drawStack.Count == 0)
        {
            cardPrefab = null;
            return false;
        }

        int lastIndex = drawStack.Count - 1;
        cardPrefab = drawStack[lastIndex];
        drawStack.RemoveAt(lastIndex);
        return true;
    }

    private bool CanDraw(int count)
    {
        int availableCards = drawStack.Count;
        if (rules.RecycleStack)
            availableCards += discardStack.Count;

        return availableCards >= count;
    }

    private void RecycleDiscardPile()
    {
        if (!rules.RecycleStack || discardStack.Count == 0)
            return;

        drawStack.AddRange(discardStack);
        discardStack.Clear();
        Shuffle(drawStack);
    }

    private bool IsValidSlot(int handIndex)
    {
        return handIndex >= 0 && handIndex < handSlots.Length;
    }

    private static void Shuffle(List<GameObject> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }
}
