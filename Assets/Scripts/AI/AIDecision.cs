public enum AIDecision
{
    Idle,
    Chase,
    Flee,
    Attack,
    Dash,
    DashAttack,
    DashGrab,
    HeavyAttack,
    Grab,
    Pummel,
    Throw,
    Shield,
    Evade,
    UseOffensiveCard,
    UseDefensiveCard,
    UseUtilityCard,
    UseBoostCard,
    Parry,
    Jump,
    ShortHop,
    // Preserve serialized values after removing the former Crouch decision (20).
    FastFall = 21,
    Reposition,
    Recover
}
