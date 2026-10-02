public struct CombatExchangeSignature
{
    public int AttackerId { get; }
    public int DefenderId { get; }
    public double FixedTime { get; }
    public bool ClashEligible { get; }

    public CombatExchangeSignature(int attackerId, int defenderId, double fixedTime, bool clashEligible)
    {
        AttackerId = attackerId;
        DefenderId = defenderId;
        FixedTime = fixedTime;
        ClashEligible = clashEligible;
    }
}
