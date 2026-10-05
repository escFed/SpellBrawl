public interface IAICardSelector
{
    AICardChoice FindBestUsableCard(CardType targetType, AIContext context, AIActionMemory memory, float now);
    bool HasUsefulCard(AIContext context);
    bool HasEmptyHand();
    bool CanRedraw();
}
