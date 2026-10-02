public struct AIActionScore
{
    public AIDecision Decision { get; }
    public float Score { get; }
    public int CardIndex { get; }
    public string CardKey { get; }

    public bool HasCard => CardIndex >= 0;

    public AIActionScore(AIDecision decision, float score, int cardIndex = -1, string cardKey = null)
    {
        Decision = decision;
        Score = score;
        CardIndex = cardIndex;
        CardKey = cardKey;
    }

    public AIActionScore WithAddedScore(float amount)
    {
        return new AIActionScore(Decision, Score + amount, CardIndex, CardKey);
    }

    public static AIActionScore Idle()
    {
        return new AIActionScore(AIDecision.Idle, 0f);
    }
}

