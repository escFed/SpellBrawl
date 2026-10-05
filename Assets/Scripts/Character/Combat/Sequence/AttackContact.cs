public class AttackContact
{
    public AttackHitbox Source { get; }
    public CharacterCoordinator Attacker { get; }
    public CharacterCoordinator Defender { get; }
    public ICombatHitReceiver Target { get; }
    public CombatHit Hit { get; }
    public AttackStats Stats { get; }
    public AerialAttackStats AerialStats { get; }
    public bool TargetGroundedBeforeHit { get; }
    public float SuspensionStrength { get; }
    public double FixedTime { get; }
    public bool ClashEligible { get; }
    public CombatExchangeSignature Signature => new CombatExchangeSignature(Attacker != null ? Attacker.GetInstanceID() : 0, Defender != null ? Defender.GetInstanceID() : 0, FixedTime, ClashEligible);

    public AttackContact(AttackHitbox source, CharacterCoordinator attacker, CharacterCoordinator defender, ICombatHitReceiver target, CombatHit hit, AttackStats stats, AerialAttackStats aerialStats, bool targetGroundedBeforeHit, float suspensionStrength, double fixedTime, bool clashEligible)
    {
        Source = source;
        Attacker = attacker;
        Defender = defender;
        Target = target;
        Hit = hit;
        Stats = stats;
        AerialStats = aerialStats;
        TargetGroundedBeforeHit = targetGroundedBeforeHit;
        SuspensionStrength = suspensionStrength;
        FixedTime = fixedTime;
        ClashEligible = clashEligible;
    }
}

