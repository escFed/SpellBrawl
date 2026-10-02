public static class CombatExchangeRules
{
    public const double DefaultClashWindow = 0.05d;

    public static bool IsMutualClash(CombatExchangeSignature first, CombatExchangeSignature second, double clashWindow = DefaultClashWindow)
    {
        if (!first.ClashEligible || !second.ClashEligible || first.AttackerId == 0 || first.DefenderId == 0 || second.AttackerId == 0 || second.DefenderId == 0)
            return false;

        bool reciprocal = first.AttackerId == second.DefenderId && first.DefenderId == second.AttackerId;
        return reciprocal && System.Math.Abs(first.FixedTime - second.FixedTime) <= System.Math.Max(0d, clashWindow);
    }
}
