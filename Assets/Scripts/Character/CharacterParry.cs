using UnityEngine;

public class CharacterParry : MonoBehaviour
{
    private const float SuccessfulParryStunDuration = 1f;

    [Header("Audio")]
    public AudioClip parrySuccessSound;
    private AudioSource audioSource;

    private CharacterCoordinator controller;

    private bool hasParriedThisHit = false;
    public bool IsParrying { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        audioSource = GetComponent<AudioSource>();
        GameSettings.RegisterSource(audioSource, GameSound.SoundEffects);
    }
    public bool TryParry()
    {
        ICharacterState currentState = controller.GetCurrentState();

        if (currentState != controller.States.Idle && currentState != controller.States.Move)
            return false;

        hasParriedThisHit = false;
        controller.ChangeState(controller.States.Parry);
        return true;
    }

    public void SetParryWindowActive(bool active)
    {
        IsParrying = active;
    }

    public void OnSuccessfulParry(CharacterCoordinator attacker)
    {
        if (hasParriedThisHit) return;

        if (audioSource != null && parrySuccessSound != null)
        {
            audioSource.PlayOneShot(parrySuccessSound);
        }

        hasParriedThisHit = true;
        CombatFeedback.PlayParryRumble(controller);

        if (attacker != null && attacker != controller && !attacker.IsDead && attacker.Combat != null)
            attacker.Combat.TakeHit(SuccessfulParryStunDuration, HitReaction.Stunned);
    }
}
