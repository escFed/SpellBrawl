public struct AIActionScore
{
    public AIDecision Decision { get; private set; }
    public float Score { get; private set; }
    public int CardIndex { get; private set; }

    public bool HasCard => CardIndex >= 0;

    public AIActionScore(AIDecision decision, float score, int cardIndex = -1)
    {
        Decision = decision;
        Score = score;
        CardIndex = cardIndex;
    }

    public void AddScore(float amount)
    {
        Score += amount;
    }

    public static AIActionScore Idle()
    {
        return new AIActionScore(AIDecision.Idle, 0f);
    }
}

