public struct HandSlotView
{
    public HandSlotView(ICardable card, bool isUnlocked, float readyAt)
    {
        Card = card;
        IsUnlocked = isUnlocked;
        ReadyAt = readyAt;
    }

    public ICardable Card { get; }
    public bool IsUnlocked { get; }
    public float ReadyAt { get; }
}