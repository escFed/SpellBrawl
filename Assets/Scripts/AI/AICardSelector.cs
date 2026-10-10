using UnityEngine;

public class AICardSelector : IAICardSelector
{
    private CharacterCoordinator selfController;
    private CharacterDeck selfDeck;
    private AITarget targetTracker;

    public void Initialize(CharacterCoordinator controller, CharacterDeck deck, AITarget target)
    {
        selfController = controller;
        selfDeck = deck;
        targetTracker = target;
    }

    public AICardChoice FindBestUsableCard(CardType targetType, AIContext context, AIActionMemory memory, float now)
    {
        if (selfDeck == null)
            return new AICardChoice(-1, 0f);

        AICardChoice best = new AICardChoice(-1, 0f);
        float bestAdjustment = float.NegativeInfinity;

        for (int i = 0; i < selfDeck.HandSlotCount; i++)
        {
            if (!selfDeck.IsSlotReady(i))
                continue;

            ICardable card = selfDeck.GetCardAt(i);

            if (card == null)
                continue;


            if (!card.CanBeUsed(selfController))
                continue;

            if (card.Type != targetType)
                continue;

            float adjustment = ScoreCard(card, context);
            if (memory != null)
                adjustment -= memory.Penalty(DecisionForType(targetType), card.GetType().Name, now);
            if (adjustment > bestAdjustment)
            {
                bestAdjustment = adjustment;
                best = new AICardChoice(i, adjustment, card.GetType().Name);
            }
        }

        return best;
    }

    public bool HasUsefulCard(AIContext context)
    {
        if (selfDeck == null)
            return false;

        for (int i = 0; i < selfDeck.HandSlotCount; i++)
        {
            if (!selfDeck.IsSlotReady(i))
                continue;

            ICardable card = selfDeck.GetCardAt(i);

            if (card == null)
                continue;


            if (!card.CanBeUsed(selfController))
                continue;

            if (ScoreCard(card, context) > -25f &&
                (card.Type != CardType.Offensive || context.TargetInCardRange))
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

    private float ScoreCard(ICardable card, AIContext context)
    {
        float score = 0f;
        if (card is HealCard)
            return context.SelfDamage >= 20f ? score + Mathf.Min(45f, context.SelfDamage * 0.5f) : -100f;
        if (card is FortifyCard)
            return selfController != null && selfController.Health != null && selfController.Health.activeDefenseMultiplier < 1f ? -100f : context.IncomingHazard || context.TargetThreatening ? score + 38f : score - 20f;
        if (card is HasteCard)
            return selfController != null && selfController.Movement != null && selfController.Movement.moveSpeedMultiplier > 1f ? -100f : score + 12f;
        if (card is FireBallCard)
            return Mathf.Abs(context.DistanceY) < 1.2f && !context.TargetShielding ? score + 14f : score - 28f;
        if (card is ThunderStrikeCard)
            return score + (context.DistanceX < 4f ? 25f : 8f);
        if (card is StarThrowCard)
            return score + (context.DistanceX > 3f ? 26f : 10f);
        if (card is ShadowSpikeCard)
            return score + (context.TargetShielding ? 4f : 24f);
        if (card is TsunamiCard)
            return Mathf.Abs(context.DistanceY) <= 1.5f && !context.TargetShielding
                ? score + (context.DistanceX <= 8f ? 25f : 8f)
                : -100f;
        if (card is BlackHoleCard)
            return score + (context.DistanceX < 4f ? 32f : 18f);
        if (card is DeckShuffleCard)
            return TargetHasCards() ? score + (context.TargetThreatening ? 16f : 8f) : -100f;
        if (card is InverseGravityCard)
            return score + (context.TargetAbove ? 22f : 6f);
        if (card is MirrorWorldCard)
            return score + (context.DistanceX < 4f ? 18f : 5f);
        return score;
    }

    private static AIDecision DecisionForType(CardType type)
    {
        switch (type)
        {
            case CardType.Offensive: return AIDecision.UseOffensiveCard;
            case CardType.Defensive: return AIDecision.UseDefensiveCard;
            case CardType.Utility: return AIDecision.UseUtilityCard;
            default: return AIDecision.UseBoostCard;
        }
    }

    private bool TargetHasCards()
    {
        CharacterCoordinator target = targetTracker != null ? targetTracker.TargetController : null;
        if (target == null)
            return false;
        CharacterDeck deck = target.GetComponent<CharacterDeck>();
        if (deck == null)
            return false;
        for (int i = 0; i < deck.HandSlotCount; i++)
        {
            if (deck.GetCardAt(i) != null)
                return true;
        }
        return false;
    }
}

