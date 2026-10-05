using System;

public class AIUtility
{
    private Random random;

    public AIUtility(Random random = null)
    {
        this.random = random ?? new Random();
    }

    public AIActionScore ChooseDecision(AIContext context, AIProfile profile, IAICardSelector cardSelector, AIDecision currentDecision, float attackRange, float idealSpacing, AIActionMemory memory = null, float now = 0f)
    {
        AIActionScore best = AIActionScore.Idle();

        // Recovery is an emergency movement plan, not a competing combat option.
        if (context.ShouldRecover)
            return ScoreRecover(context, profile);

        TryChoose(ref best, ScoreRecover(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreDrawCards(context, cardSelector), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreEvade(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreShield(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreParry(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreHeavyAttack(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreDashAttack(context, profile, attackRange, idealSpacing), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreAttack(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreDash(context, profile, idealSpacing), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreShortHop(context), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreJump(context), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreFastFall(context), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreCrouch(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreFlee(context, profile), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreReposition(context, attackRange, idealSpacing), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreChase(context, profile, attackRange, idealSpacing), profile, currentDecision, memory, now);

        TryChoose(ref best, ScoreCard(AIDecision.UseDefensiveCard, CardType.Defensive, ScoreDefensiveCard(context, profile), cardSelector, context, memory, now), profile, currentDecision, memory, now);
        TryChoose(ref best, ScoreCard(AIDecision.UseOffensiveCard, CardType.Offensive, ScoreOffensiveCard(context, profile, attackRange), cardSelector, context, memory, now), profile, currentDecision, memory, now);
        TryChoose(
            ref best,
            ScoreCard(
                AIDecision.UseUtilityCard,
                CardType.Utility,
                ScoreUtilityCard(context, profile, attackRange),
                cardSelector, context, memory, now),
            profile,
            currentDecision, memory, now);
        TryChoose(
            ref best,
            ScoreCard(
                AIDecision.UseBoostCard,
                CardType.Boost,
                ScoreBoostCard(context, profile),
                cardSelector, context, memory, now),
            profile,
            currentDecision, memory, now);

        if (context.CanMove && context.CanChaseSafely && !context.IncomingHazard &&
            !context.InDanger && !context.TargetThreatening &&
            Next01() < profile.mistakeChance)
            return new AIActionScore(AIDecision.Chase, 1f);

        return best;
    }

    private void TryChoose(
        ref AIActionScore best,
        AIActionScore candidate,
        AIProfile profile,
        AIDecision currentDecision,
        AIActionMemory memory,
        float now)
    {
        if (candidate.Score <= 0f)
            return;

        candidate = candidate.WithAddedScore(Range(-profile.randomness, profile.randomness));

        if (candidate.Decision == currentDecision)
            candidate = candidate.WithAddedScore(10f);

        if (memory != null && !candidate.HasCard)
            candidate = candidate.WithAddedScore(-memory.Penalty(candidate.Decision, candidate.CardKey, now));

        if (candidate.Score > best.Score)
            best = candidate;
    }

    private static AIActionScore ScoreCard(
        AIDecision decision,
        CardType cardType,
        float score,
        IAICardSelector cardSelector,
        AIContext context,
        AIActionMemory memory,
        float now)
    {
        if (score <= 0f)
            return new AIActionScore(decision, 0f);

        AICardChoice choice = cardSelector.FindBestUsableCard(cardType, context, memory, now);
        return choice.Index >= 0 && score + choice.ScoreAdjustment > 0f
            ? new AIActionScore(decision, score + choice.ScoreAdjustment, choice.Index, choice.Key)
            : new AIActionScore(decision, 0f);
    }

    private static AIActionScore ScoreRecover(AIContext context, AIProfile profile)
    {
        return context.ShouldRecover
            ? new AIActionScore(AIDecision.Recover, 100f * profile.recoveryFocus)
            : new AIActionScore(AIDecision.Recover, 0f);
    }

    private static AIActionScore ScoreAttack(AIContext context, AIProfile profile)
    {
        if (!context.CanAttack || !context.PredictedAttackHit)
            return new AIActionScore(AIDecision.Attack, 0f);

        float score = 55f;
        if (context.TargetDamage >= 100f)
            score += 20f;
        if (context.SelfDamage >= 120f)
            score -= 15f;

        return new AIActionScore(AIDecision.Attack, score * profile.aggression);
    }

    private static AIActionScore ScoreChase(
        AIContext context,
        AIProfile profile,
        float attackRange,
        float idealSpacing)
    {
        if (!context.CanMove || !context.CanChaseSafely ||
            (context.DistanceX <= idealSpacing && !context.HasPlannedJump))
            return new AIActionScore(AIDecision.Chase, 0f);
        if (context.InDanger && context.DistanceX < attackRange * 2f)
            return new AIActionScore(AIDecision.Chase, 8f);

        return new AIActionScore(AIDecision.Chase, 28f * profile.aggression);
    }

    private static AIActionScore ScoreFlee(AIContext context, AIProfile profile)
    {
        if (!context.CanMove || !context.CanFleeSafely)
            return new AIActionScore(AIDecision.Flee, 0f);

        float score = 0f;
        if (context.InDanger)
            score += 35f;
        if (context.TargetInAttackRange)
            score += 30f;
        if (context.NearEdge)
            score += 20f;

        return new AIActionScore(AIDecision.Flee, score * profile.defense);
    }

    private static AIActionScore ScoreJump(AIContext context)
    {
        return context.CanJump && context.HasPlannedJump && context.TargetAbove && context.DistanceX < 4f
            ? new AIActionScore(AIDecision.Jump, 50f)
            : new AIActionScore(AIDecision.Jump, 0f);
    }

    private static AIActionScore ScoreShortHop(AIContext context)
    {
        return context.IsGrounded && context.CanJump && context.CanShortHopSafely && context.DistanceY > 0.35f &&
            context.DistanceY <= 1.5f && context.DistanceX < 3f
            ? new AIActionScore(AIDecision.ShortHop, 52f)
            : new AIActionScore(AIDecision.ShortHop, 0f);
    }

    private static AIActionScore ScoreFastFall(AIContext context)
    {
        return context.CanFastFall && context.CanFastFallSafely && context.TargetBelow
            ? new AIActionScore(AIDecision.FastFall, 52f)
            : new AIActionScore(AIDecision.FastFall, 0f);
    }

    private static AIActionScore ScoreCrouch(AIContext context, AIProfile profile)
    {
        return context.CanCrouch && context.TargetThreatening && context.TargetAbove
            ? new AIActionScore(AIDecision.Crouch, 48f * profile.defense)
            : new AIActionScore(AIDecision.Crouch, 0f);
    }

    private static AIActionScore ScoreDash(AIContext context, AIProfile profile, float idealSpacing)
    {
        if (!context.CanDash || !context.CanDashSafely || context.DistanceX <= idealSpacing * 1.5f)
            return new AIActionScore(AIDecision.Dash, 0f);

        return new AIActionScore(AIDecision.Dash, 45f * profile.dashUsage);
    }

    private static AIActionScore ScoreDashAttack(
        AIContext context,
        AIProfile profile,
        float attackRange,
        float idealSpacing)
    {
        bool inDashAttackBand = context.DistanceX > attackRange * 0.75f &&
            context.DistanceX <= idealSpacing * 2f && Math.Abs(context.DistanceY) < 1f;
        return context.CanDash && context.CanDashSafely && inDashAttackBand && !context.TargetShielding
            ? new AIActionScore(AIDecision.DashAttack, 62f * profile.dashUsage)
            : new AIActionScore(AIDecision.DashAttack, 0f);
    }

    private static AIActionScore ScoreHeavyAttack(AIContext context, AIProfile profile)
    {
        if (!context.CanHeavyAttack || !context.PredictedHeavyHit || context.TargetThreatening || context.IncomingHazard)
            return new AIActionScore(AIDecision.HeavyAttack, 0f);

        float score = 65f + Math.Min(30f, context.TargetDamage * 0.25f);
        return new AIActionScore(AIDecision.HeavyAttack, score * profile.heavyAttackUsage);
    }

    private static AIActionScore ScoreShield(AIContext context, AIProfile profile)
    {
        if (!context.CanShield || (!context.IncomingHazard &&
            (!context.TargetThreatening || !context.TargetInAttackRange)))
            return new AIActionScore(AIDecision.Shield, 0f);

        float score = context.IncomingHazard ? 92f : context.InDanger ? 82f : 68f;
        return new AIActionScore(AIDecision.Shield, score * profile.shieldUsage);
    }

    private static AIActionScore ScoreEvade(AIContext context, AIProfile profile)
    {
        if (!context.CanEvade || !context.CanEvadeSafely || (!context.IncomingHazard &&
            (!context.TargetThreatening || !context.TargetInAttackRange)))
            return new AIActionScore(AIDecision.Evade, 0f);

        float score = context.IncomingHazard ? 100f : context.InDanger ? 92f : 76f;
        if (context.NearEdge && context.IsGrounded)
            score -= 20f;

        return new AIActionScore(AIDecision.Evade, score * profile.evadeSkill);
    }

    private static AIActionScore ScoreReposition(
        AIContext context,
        float attackRange,
        float idealSpacing)
    {
        if (!context.CanMove || !context.CanFleeSafely)
            return new AIActionScore(AIDecision.Reposition, 0f);

        if (context.DistanceX < attackRange && context.SelfDamage > 70f)
            return new AIActionScore(AIDecision.Reposition, 35f);
        if (context.DistanceX < idealSpacing)
            return new AIActionScore(AIDecision.Reposition, 20f);

        return new AIActionScore(AIDecision.Reposition, 0f);
    }

    private static AIActionScore ScoreParry(AIContext context, AIProfile profile)
    {
        if (!context.CanParry || context.IsParrying || (!context.IncomingHazard &&
            (!context.TargetInAttackRange || !context.TargetThreatening)))
            return new AIActionScore(AIDecision.Parry, 0f);

        float score = 25f;
        if (context.InDanger)
            score += 20f;
        if (context.IncomingHazard)
            score += 25f;

        return new AIActionScore(AIDecision.Parry, score * profile.parrySkill);
    }

    private static AIActionScore ScoreDrawCards(AIContext context, IAICardSelector cardSelector)
    {
        if (!context.CanUseCards || !cardSelector.CanRedraw())
            return new AIActionScore(AIDecision.DrawCards, 0f);
        if (context.EmptyHand)
            return new AIActionScore(AIDecision.DrawCards, 80f);
        if (!cardSelector.HasUsefulCard(context))
            return new AIActionScore(AIDecision.DrawCards, 45f);

        return new AIActionScore(AIDecision.DrawCards, 0f);
    }

    private static float ScoreOffensiveCard(
        AIContext context,
        AIProfile profile,
        float attackRange)
    {
        if (!context.CanUseCards || !context.TargetInCardRange)
            return 0f;

        float score = 45f;
        if (context.DistanceX > attackRange)
            score += 15f;
        if (context.TargetDamage >= 80f)
            score += 20f;

        return score * profile.cardUsage;
    }

    private static float ScoreDefensiveCard(AIContext context, AIProfile profile)
    {
        if (!context.CanUseCards)
            return 0f;

        float score = 0f;
        if (context.InDanger || context.IncomingHazard)
            score += 60f;
        if (context.TargetInAttackRange)
            score += 25f;
        if (context.NearEdge)
            score += 20f;

        return score * profile.defense;
    }

    private static float ScoreUtilityCard(
        AIContext context,
        AIProfile profile,
        float attackRange)
    {
        if (!context.CanUseCards)
            return 0f;

        float score = 20f;
        if (context.TargetAbove)
            score += 25f;
        if (context.ShouldRecover)
            score += 40f;
        if (context.DistanceX > attackRange && context.TargetInCardRange)
            score += 15f;

        return score * profile.cardUsage;
    }

    private static float ScoreBoostCard(AIContext context, AIProfile profile)
    {
        if (!context.CanUseCards)
            return 0f;

        float score = 25f;
        if (context.InDanger || context.IncomingHazard)
            score += 35f;
        if (context.SelfDamage >= 35f)
            score += Math.Min(35f, context.SelfDamage * 0.25f);

        return score * profile.boostUsage;
    }

    private float Next01()
    {
        return (float)random.NextDouble();
    }

    private float Range(float minimum, float maximum)
    {
        return minimum + (maximum - minimum) * Next01();
    }
}
