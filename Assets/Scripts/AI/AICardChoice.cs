public struct AICardChoice
{
    public int Index { get; }
    public float ScoreAdjustment { get; }
    public string Key { get; }

    public AICardChoice(int index, float scoreAdjustment, string key = null)
    {
        Index = index;
        ScoreAdjustment = scoreAdjustment;
        Key = key;
    }
}
