using System.Collections;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private AttackStats currentStats;
    private int currentDamage;
    private Vector2 currentKnockback;
    private float currentHitStun;
    private float currentGrowth;
    private HitReaction currentHitReaction;
    private Collider2D hitCollider;
    private CharacterCoordinator owner;
    private bool hasHit;
    private bool swingActive;
    private bool swingResolved;
    private int swingSequence;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        owner = GetComponentInParent<CharacterCoordinator>();
    }

    public void Setup(NormalAttackStats stats)
    {
        Setup(stats, stats != null ? stats.damage : 0, stats != null ? stats.knockback : Vector2.zero);
    }

    public void Setup(AttackStats stats, int damage, Vector2 knockback)
    {
        Setup(stats, damage, knockback, stats != null ? stats.hitStun : 0f);
    }

    public void Setup(AttackStats stats, int damage, Vector2 knockback, float hitStun, float growthOverride = -1f)
    {
        currentStats = stats;
        currentDamage = Mathf.Max(0, damage);
        currentKnockback = knockback;
        currentGrowth = growthOverride >= 0f ? growthOverride : stats?.launch?.growth ?? 3f;
        currentHitStun = Mathf.Max(0f, hitStun);
        currentHitReaction = stats != null ? stats.hitReaction : HitReaction.Hit;
        hasHit = false;
    }

    public void BeginSwing()
    {
        if (swingActive)
            return;

        swingActive = true;
        swingResolved = false;
        hasHit = false;
        swingSequence++;
        hitCollider.enabled = true;
        if (owner != null)
            MatchHitStats.AttemptRegister(owner.Mode == PlayerMode.AI ? "ai" : "player");
    }

    public void EndSwing()
    {
        if (!swingActive)
            return;

        swingActive = false;
        hitCollider.enabled = false;
        StartCoroutine(ReportMissAfterContactResolution(swingSequence));
    }

    private IEnumerator ReportMissAfterContactResolution(int sequence)
    {
        yield return new WaitForEndOfFrame();
        if (sequence != swingSequence || swingResolved || owner == null)
            yield break;

        swingResolved = true;
        string attackerType = owner.Mode == PlayerMode.AI ? "ai" : "player";
        _ = FailedHitsEvents.Report(1, attackerType, MatchHitStats.MatchId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || currentStats == null)
            return;

        if (other.transform.root == transform.root)
            return;

        ICombatHitReceiver target = other.GetComponentInParent<ICombatHitReceiver>();
        if (target == null)
            return;

        if (target is CharacterHealth health && health.IsRespawnProtected)
            return;

        float attackerDirection = owner != null && owner.Combat != null
            ? owner.Combat.FacingSign
            : Mathf.Sign(transform.root.localScale.x);
        if (attackerDirection == 0f)
            attackerDirection = 1f;

        Vector2 directedKnockback = new Vector2(currentKnockback.x * attackerDirection, currentKnockback.y);
        int attackerPlayerIndex = owner != null ? owner.PlayerIndex : -1;
        AerialAttackStats aerialStats = currentStats as AerialAttackStats;
        bool targetGroundedBeforeHit = false;
        float suspensionStrength = owner != null && aerialStats != null
            ? owner.Combat.GetAerialSuspensionStrength(target, aerialStats, out targetGroundedBeforeHit)
            : 0f;
        AerialSuspension defenderSuspension = aerialStats != null
            ? aerialStats.CreateDefenderSuspension(suspensionStrength)
            : default;

        Vector2 hitPoint = other.ClosestPoint(hitCollider.bounds.center);
        CombatHit hit = new CombatHit(currentDamage, directedKnockback, currentHitStun, currentHitReaction, hitPoint, attackerPlayerIndex, currentStats.launch, currentGrowth, defenderSuspension, owner);

        CharacterCoordinator defender = other.GetComponentInParent<CharacterCoordinator>();
        AttackContact contact = new AttackContact(this, owner, defender, target, hit, currentStats, aerialStats, targetGroundedBeforeHit, suspensionStrength, Time.fixedTimeAsDouble, IsEligibleForGroundClash(defender));

        CombatExchangeResolver.Register(contact);
        hasHit = true;
    }

    public void ResolveContact(AttackContact contact)
    {
        if (contact == null || contact.Target == null)
            return;

        if (contact.Target is Object targetObject && targetObject == null)
            return;

        bool applied = contact.Target.ReceiveHit(contact.Hit);
        if (applied)
        {
            swingResolved = true;
            if (contact.Attacker != null)
            {
                string attackerType = contact.Attacker.Mode == PlayerMode.AI ? "ai" : "player";
                MatchHitStats.HitRegister(attackerType);
            }

            contact.Attacker?.Combat.RegisterForwardTiltHit(contact.Source, contact.Stats);

            if (contact.Attacker != null && contact.Stats is GroundAttackStats groundStats && groundStats.startsAerialCombo)
                contact.Attacker.Combat.BeginAerialCombo(contact.Target);

            if (contact.Attacker != null && contact.AerialStats != null)
            {
                if (!contact.Attacker.IsGrounded)
                    contact.Attacker.Movement.BeginAerialSuspension(
                        contact.AerialStats.CreateAttackerSuspension(contact.SuspensionStrength));
                contact.Attacker.Combat.RegisterAerialHit(contact.Target, contact.TargetGroundedBeforeHit);
            }

            CombatFeedback.PlayHitSound(contact.Stats.hitSound);
            if (contact.Attacker != null)
                _ = SuccessfulHitsEvent.Report(
                    1,
                    contact.Attacker.Mode == PlayerMode.AI ? "ai" : "player",
                    MatchHitStats.MatchId);

            if (contact.Attacker != null)
                _ = HitAdditionToKnockBack.Report(contact.Hit.BaseKnockback.magnitude,
                    contact.Hit.Damage,
                    contact.Attacker.Mode == PlayerMode.AI ? "ai" : "player",
                    MatchHitStats.MatchId);
        }
        else
        {
            swingResolved = true;
            if (contact.Attacker != null)
                _ = FailedHitsEvents.Report(1,
                    contact.Attacker.Mode == PlayerMode.AI ? "ai" : "player",
                    MatchHitStats.MatchId);
        }
    }

    private bool IsEligibleForGroundClash(CharacterCoordinator defender)
    {
        if (currentStats is not GroundAttackStats || owner == null || defender == null || owner == defender)
            return false;

        CharacterHealth ownerHealth = owner.Health;
        CharacterHealth defenderHealth = defender.Health;
        if (ownerHealth == null || defenderHealth == null ||
            ownerHealth.Phase != RespawnPhase.Active || defenderHealth.Phase != RespawnPhase.Active ||
            ownerHealth.IsIntangible || defenderHealth.IsIntangible)
            return false;

        if (!owner.Movement.HasStableGroundContact || !defender.Movement.HasStableGroundContact)
            return false;

        if (owner.IsParrying || defender.IsParrying)
            return false;

        return (owner.Shield == null || !owner.Shield.IsActive) &&
            (defender.Shield == null || !defender.Shield.IsActive);
    }

    private void OnDrawGizmos()
    {
        Collider2D colliderToDraw = GetComponent<Collider2D>();

        if (colliderToDraw != null && colliderToDraw.enabled)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(colliderToDraw.bounds.center, colliderToDraw.bounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(colliderToDraw.bounds.center, colliderToDraw.bounds.size);
        }
    }
}
