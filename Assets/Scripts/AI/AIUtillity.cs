using UnityEngine;

public class AIUtilityBrain
{
    private AIProfile profile;
    private AICardSelector cardSelector;
    private PlayerController selfController;
    private AIDecision currentDecision;
    private float attackRange;
    private float idealSpacing;
    private float cardCooldownTimer;
    private float attackCooldownTimer;

    public AIActionScore ChooseDecision(AIContext context, AIProfile profile, AICardSelector cardSelector, PlayerController selfController, AIDecision currentDecision, float attackRange, float idealSpacing, float cardCooldownTimer, float attackCooldownTimer)
    {
        this.profile = profile;
        this.cardSelector = cardSelector;
        this.selfController = selfController;
        this.currentDecision = currentDecision;
        this.attackRange = attackRange;
        this.idealSpacing = idealSpacing;
        this.cardCooldownTimer = cardCooldownTimer;
        this.attackCooldownTimer = attackCooldownTimer;

        AIActionScore best = AIActionScore.Idle();

        TryChoose(ref best, ScoreRecover(context));
        TryChoose(ref best, ScoreDrawCards(context));
        TryChoose(ref best, ScoreParry(context));
        TryChoose(ref best, ScoreAttack(context));
        TryChoose(ref best, ScoreJump(context));
        TryChoose(ref best, ScoreFlee(context));
        TryChoose(ref best, ScoreReposition(context));
        TryChoose(ref best, ScoreChase(context));

        TryChoose(ref best, ScoreCard(AIDecision.UseDefensiveCard, CardType.DEFENSIVE, ScoreDefensiveCard(context)));
        TryChoose(ref best, ScoreCard(AIDecision.UseOffensiveCard, CardType.OFFENSIVE, ScoreOffensiveCard(context)));
        TryChoose(ref best, ScoreCard(AIDecision.UseUtilityCard, CardType.UTILITY, ScoreUtilityCard(context)));

        if (Random.value < profile.mistakeChance)
            return new AIActionScore(AIDecision.Chase, 1f);

        return best;
    }

    private void TryChoose(ref AIActionScore best, AIActionScore candidate)
    {
        if (candidate.Score <= 0f)
            return;

        candidate.AddScore(Random.Range(-profile.randomness, profile.randomness));

        if (candidate.Decision == currentDecision)
            candidate.AddScore(10f);

        if (candidate.Score > best.Score)
            best = candidate;
    }

    private AIActionScore ScoreCard(AIDecision decision, CardType cardType, float score)
    {
        if (score <= 0f)
            return new AIActionScore(decision, 0f);

        int cardIndex = cardSelector.FindFirstUsableCard(cardType);

        if (cardIndex < 0)
            return new AIActionScore(decision, 0f);

        return new AIActionScore(decision, score, cardIndex);
    }

    private AIActionScore ScoreRecover(AIContext context)
    {
        if (!context.shouldRecover)
            return new AIActionScore(AIDecision.Recover, 0f);

        return new AIActionScore(AIDecision.Recover, 100f * profile.recoveryFocus);
    }

    private AIActionScore ScoreAttack(AIContext context)
    {
        if (attackCooldownTimer > 0f || !context.targetInAttackRange)
            return new AIActionScore(AIDecision.Attack, 0f);

        float score = 55f;

        if (context.targetDamage >= 100f)
            score += 20f;

        if (context.selfDamage >= 120f)
            score -= 15f;

        return new AIActionScore(AIDecision.Attack, score * profile.aggression);
    }

    private AIActionScore ScoreChase(AIContext context)
    {
        if (context.distanceX <= idealSpacing)
            return new AIActionScore(AIDecision.Chase, 0f);

        if (context.inDanger && context.distanceX < attackRange * 2f)
            return new AIActionScore(AIDecision.Chase, 10f);

        return new AIActionScore(AIDecision.Chase, 35f * profile.aggression);
    }

    private AIActionScore ScoreFlee(AIContext context)
    {
        float score = 0f;

        if (context.inDanger)
            score += 35f;

        if (context.targetInAttackRange)
            score += 30f;

        if (context.nearEdge)
            score += 20f;

        return new AIActionScore(AIDecision.Flee, score * profile.defense);
    }

    private AIActionScore ScoreJump(AIContext context)
    {
        if (context.targetAbove && context.distanceX < 4f)
            return new AIActionScore(AIDecision.Jump, 50f);

        return new AIActionScore(AIDecision.Jump, 0f);
    }

    private AIActionScore ScoreReposition(AIContext context)
    {
        if (context.distanceX < attackRange && context.selfDamage > 70f)
            return new AIActionScore(AIDecision.Reposition, 35f);

        if (context.distanceX < attackRange * 1.25f)
            return new AIActionScore(AIDecision.Reposition, 20f);

        return new AIActionScore(AIDecision.Reposition, 0f);
    }

    private AIActionScore ScoreParry(AIContext context)
    {
        if (!context.targetInAttackRange || selfController.IsParrying)
            return new AIActionScore(AIDecision.Parry, 0f);

        float score = 25f;

        if (context.inDanger)
            score += 20f;

        if (context.energy < 40f)
            score += 15f;

        return new AIActionScore(AIDecision.Parry, score * profile.parrySkill);
    }

    private AIActionScore ScoreDrawCards(AIContext context)
    {
        if (context.energy < 75f)
            return new AIActionScore(AIDecision.DrawCards, 0f);

        if (context.emptyHand)
            return new AIActionScore(AIDecision.DrawCards, 80f);

        if (!cardSelector.HasUsefulCard(context) && context.energy >= 100f)
            return new AIActionScore(AIDecision.DrawCards, 45f);

        return new AIActionScore(AIDecision.DrawCards, 0f);
    }

    private float ScoreOffensiveCard(AIContext context)
    {
        if (cardCooldownTimer > 0f || !context.targetInCardRange)
            return 0f;

        float score = 45f;

        if (context.distanceX > attackRange)
            score += 15f;

        if (context.targetDamage >= 80f)
            score += 20f;

        if (context.energy >= 80f)
            score += 10f;

        return score * profile.cardUsage;
    }

    private float ScoreDefensiveCard(AIContext context)
    {
        if (cardCooldownTimer > 0f)
            return 0f;

        float score = 0f;

        if (context.inDanger)
            score += 60f;

        if (context.targetInAttackRange)
            score += 25f;

        if (context.nearEdge)
            score += 20f;

        return score * profile.defense;
    }

    private float ScoreUtilityCard(AIContext context)
    {
        if (cardCooldownTimer > 0f)
            return 0f;

        float score = 20f;

        if (context.targetAbove)
            score += 25f;

        if (context.shouldRecover)
            score += 40f;

        if (context.distanceX > attackRange && context.targetInCardRange)
            score += 15f;

        return score * profile.cardUsage;
    }
}

