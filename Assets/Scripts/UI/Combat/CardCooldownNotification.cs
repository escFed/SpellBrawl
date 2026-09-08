// Presentation state only: the deck remains the owner of card readiness.
public struct CardCooldownNotification
{
    private ICardable card;
    private float readyAt;
    private bool pending;

    public void Observe(HandSlotView slot, float currentTime)
    {
        if (!slot.IsUnlocked || slot.Card == null)
        {
            this = default;
            return;
        }

        if (!object.ReferenceEquals(card, slot.Card) || readyAt != slot.ReadyAt)
        {
            card = slot.Card;
            readyAt = slot.ReadyAt;
            // Initial hands and already-ready cards do not announce a cooldown completion.
            pending = readyAt > currentTime;
        }
    }

    public bool TryConsume(float currentTime)
    {
        if (!pending || currentTime < readyAt)
            return false;

        pending = false;
        return true;
    }
}
