public static class ModeRules
{
    public static PlayerMode GetControlMode(MatchMode matchMode, PlayerSlot slot)
    {
        switch (matchMode)
        {
            case MatchMode.PlayerVsPlayer:
                return PlayerMode.Player;
            case MatchMode.PlayerVsAI:
                return slot == PlayerSlot.PlayerOne ? PlayerMode.Player : PlayerMode.AI;
            case MatchMode.AIVsAI:
                return PlayerMode.AI;
            default:
                return PlayerMode.Player;
        }
    }

    public static bool UsesCustomDeck(MatchMode matchMode, PlayerSlot slot)
    {
        return matchMode == MatchMode.PlayerVsPlayer || (matchMode == MatchMode.PlayerVsAI && slot == PlayerSlot.PlayerOne);
    }

    public static string GetDisplayName(MatchMode matchMode, PlayerSlot slot)
    {
        switch (matchMode)
        {
            case MatchMode.PlayerVsPlayer:
                return slot == PlayerSlot.PlayerOne ? "Player 1" : "Player 2";
            case MatchMode.PlayerVsAI:
                return slot == PlayerSlot.PlayerOne ? "Player 1" : "AI";
            case MatchMode.AIVsAI:
                return slot == PlayerSlot.PlayerOne ? "AI 1" : "AI 2";
            default:
                return slot == PlayerSlot.PlayerOne ? "Player 1" : "Player 2";
        }
    }
}
