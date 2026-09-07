public struct HandSlotView
{
    public HandSlotView(ICardable card, bool isUnlocked, float readyAt, float cooldownDuration)
    {
        Card = card;
        IsUnlocked = isUnlocked;
        ReadyAt = readyAt;
        CooldownDuration = cooldownDuration;
    }

    public ICardable Card { get; }
    public bool IsUnlocked { get; }
    public float ReadyAt { get; }
    public float CooldownDuration { get; }
}
