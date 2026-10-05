using System.Collections.Generic;
using UnityEngine;

public class CombatExchangeResolver : MonoBehaviour
{
    private const float SequenceStepDuration = 0.40f;
    private const float LaunchHorizontalSpeed = 8f;
    private const float LaunchVerticalSpeed = 3f;
    private const float LaunchHitStun = 1f;

    private static CombatExchangeResolver instance;

    private List<AttackContact> pendingContacts = new();

    public static void Register(AttackContact contact)
    {
        if (contact == null || contact.Source == null)
            return;

        GetOrCreate().pendingContacts.Add(contact);
    }

    public static bool AreMutualClashContacts(AttackContact first, AttackContact second, double clashWindow = CombatExchangeRules.DefaultClashWindow)
    {
        if (first == null || second == null || !first.ClashEligible || !second.ClashEligible)
            return false;

        if (first.Attacker == null || first.Defender == null || second.Attacker == null || second.Defender == null)
            return false;

        return CombatExchangeRules.IsMutualClash(first.Signature, second.Signature, clashWindow);
    }

    private static CombatExchangeResolver GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject resolverObject = new GameObject(nameof(CombatExchangeResolver));
        instance = resolverObject.AddComponent<CombatExchangeResolver>();
        return instance;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void LateUpdate()
    {
        if (pendingContacts.Count == 0)
            return;

        ResolvePendingContacts();
        pendingContacts.Clear();
    }

    private void ResolvePendingContacts()
    {
        bool[] resolved = new bool[pendingContacts.Count];

        for (int firstIndex = 0; firstIndex < pendingContacts.Count; firstIndex++)
        {
            if (resolved[firstIndex])
                continue;

            AttackContact first = pendingContacts[firstIndex];
            for (int secondIndex = firstIndex + 1; secondIndex < pendingContacts.Count; secondIndex++)
            {
                if (resolved[secondIndex])
                    continue;

                AttackContact second = pendingContacts[secondIndex];
                if (!AreMutualClashContacts(first, second) || !CanStartClash(first.Attacker, first.Defender))
                    continue;

                resolved[firstIndex] = true;
                resolved[secondIndex] = true;
                StartClash(first.Attacker, first.Defender);
                break;
            }
        }

        for (int index = 0; index < pendingContacts.Count; index++)
        {
            if (!resolved[index] && pendingContacts[index].Source != null)
                pendingContacts[index].Source.ResolveContact(pendingContacts[index]);
        }
    }

    private static bool CanStartClash(CharacterCoordinator first, CharacterCoordinator second)
    {
        if (first == null || second == null || first.IsDead || second.IsDead || first.IsInClashSequence || second.IsInClashSequence)
            return false;

        if (first.Health == null || second.Health == null || first.Health.Phase != RespawnPhase.Active || second.Health.Phase != RespawnPhase.Active || first.Health.IsIntangible || second.Health.IsIntangible)
            return false;

        return first.Movement.HasStableGroundContact && second.Movement.HasStableGroundContact && !first.IsParrying && !second.IsParrying && (first.Shield == null || !first.Shield.IsActive) && (second.Shield == null || !second.Shield.IsActive);
    }

    private static void StartClash(CharacterCoordinator first, CharacterCoordinator second)
    {
        float firstDirection = GetOutwardDirection(first, second);
        float secondDirection = -firstDirection;

        first.States.ClashSequence.Prepare(second, new Vector2(firstDirection * LaunchHorizontalSpeed, LaunchVerticalSpeed), SequenceStepDuration, LaunchHitStun);
        second.States.ClashSequence.Prepare(first, new Vector2(secondDirection * LaunchHorizontalSpeed, LaunchVerticalSpeed), SequenceStepDuration, LaunchHitStun);

        first.ChangeState(first.States.ClashSequence);
        second.ChangeState(second.States.ClashSequence);

        Vector2 impactPoint = (first.transform.position + second.transform.position) * 0.5f;
        CombatFeedback.PlayHitSound();
        CombatFeedback.PlayImpact(impactPoint, Vector2.up * LaunchVerticalSpeed, HitReaction.Stunned, Color.white);
    }

    private static float GetOutwardDirection(CharacterCoordinator character, CharacterCoordinator opponent)
    {
        float direction = Mathf.Sign(character.transform.position.x - opponent.transform.position.x);
        if (direction != 0f)
            return direction;

        return character.PlayerIndex <= opponent.PlayerIndex ? -1f : 1f;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
