using UnityEngine;

// A small, temporary penalty for repeating an attack that failed to deal damage.
public class AIActionMemory
{
    private const int Capacity = 4;
    private Attempt[] attempts = new Attempt[Capacity];
    private int next;

    private struct Attempt
    {
        public AIDecision Decision;
        public string CardKey;
        public int DamageBefore;
        public float StartedAt;
        public float ResolveAt;
        public bool Active;
        public bool Failed;
    }

    public void Reset()
    {
        for (int i = 0; i < attempts.Length; i++)
            attempts[i] = default;
        next = 0;
    }

    public void Record(AIActionScore action, int targetDamage, float now)
    {
        if (!TracksDamage(action.Decision, action.CardKey))
            return;

        attempts[next] = new Attempt
        {
            Decision = action.Decision,
            CardKey = action.CardKey,
            DamageBefore = targetDamage,
            StartedAt = now,
            ResolveAt = now + (action.HasCard ? 1.5f : 0.8f),
            Active = true
        };
        next = (next + 1) % Capacity;
    }

    public void Observe(int targetDamage, float now)
    {
        for (int i = 0; i < attempts.Length; i++)
        {
            Attempt attempt = attempts[i];
            if (!attempt.Active || attempt.Failed)
                continue;
            if (targetDamage > attempt.DamageBefore)
                attempt.Active = false;
            else if (now >= attempt.ResolveAt)
                attempt.Failed = true;
            attempts[i] = attempt;
        }
    }

    public float Penalty(AIDecision decision, string cardKey, float now)
    {
        float penalty = 0f;
        for (int i = 0; i < attempts.Length; i++)
        {
            Attempt attempt = attempts[i];
            if (!attempt.Active || attempt.Decision != decision || attempt.CardKey != cardKey)
                continue;

            if (!attempt.Failed)
            {
                if (now <= attempt.ResolveAt)
                    penalty = Mathf.Max(penalty, 12f);
                continue;
            }

            float age = now - attempt.ResolveAt;
            if (age < 3f)
                penalty = Mathf.Max(penalty, 32f * (1f - Mathf.Clamp01(age / 3f)));
        }
        return penalty;
    }

    private static bool TracksDamage(AIDecision decision, string cardKey)
    {
        return decision == AIDecision.Attack || decision == AIDecision.DashAttack || decision == AIDecision.HeavyAttack || (decision == AIDecision.UseOffensiveCard && cardKey != nameof(TsunamiCard));
    }
}
