using UnityEngine;

public class HandSlot
{
    public GameObject CardPrefab { get; private set; }
    public ICardable Card { get; private set; }
    public bool IsUnlocked { get; private set; }
    public float ReadyAt { get; private set; }

    public float cooldownDuration { get; private set; }


    public bool HasCard => Card != null;

    public void Unlock()
    {
        IsUnlocked = true;
    }

    public void Lock()
    {
        Clear();
        IsUnlocked = false;
    }

    public void Assign(GameObject cardPrefab, ICardable card, float readyAt)
    {
        CardPrefab = cardPrefab;
        Card = card;
        ReadyAt = readyAt;
    }

    public void Clear()
    {
        CardPrefab = null;
        Card = null;
        ReadyAt = 0f;
    }

    public bool IsReady(float currentTime)
    {
        return IsUnlocked && HasCard && currentTime >= ReadyAt;
    }

    public HandSlotView CreateView()
    {
        return new HandSlotView(Card, IsUnlocked, ReadyAt, cooldownDuration);
    }
}
