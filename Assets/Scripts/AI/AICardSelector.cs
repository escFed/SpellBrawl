public class AICardSelector
{
    private PlayerController selfController;
    private EnergyManager selfEnergy;
    private CharacterDeck selfDeck;

    public void Initialize(PlayerController controller, EnergyManager energy, CharacterDeck deck)
    {
        selfController = controller;
        selfEnergy = energy;
        selfDeck = deck;
    }

    public int FindFirstUsableCard(CardType targetType)
    {
        if (selfDeck == null || selfEnergy == null)
            return -1;

        for (int i = 0; i < selfDeck.HandSlotCount; i++)
        {
            if (!selfDeck.IsSlotReady(i))
                continue;

            ICardable card = selfDeck.GetCardAt(i);

            if (card == null)
                continue;

            if (selfEnergy.currentEnergy < card.EnergyCost)
                continue;

            if (!card.CanBeUsed(selfController))
                continue;

            if (card.Type == targetType)
                return i;
        }

        return -1;
    }

    public bool HasUsefulCard(AIContext context)
    {
        if (selfDeck == null || selfEnergy == null)
            return false;

        for (int i = 0; i < selfDeck.HandSlotCount; i++)
        {
            if (!selfDeck.IsSlotReady(i))
                continue;

            ICardable card = selfDeck.GetCardAt(i);

            if (card == null)
                continue;

            if (selfEnergy.currentEnergy < card.EnergyCost)
                continue;

            if (!card.CanBeUsed(selfController))
                continue;

            if (card.Type == CardType.Defensive && context.inDanger)
                return true;

            if (card.Type == CardType.Offensive && context.targetInCardRange)
                return true;

            if (card.Type == CardType.Utility)
                return true;
        }

        return false;
    }

    public bool HasEmptyHand()
    {
        if (selfDeck == null)
            return true;

        for (int i = 0; i < selfDeck.HandSlotCount; i++)
        {
            if (selfDeck.GetCardAt(i) != null)
                return false;
        }

        return true;
    }
}

