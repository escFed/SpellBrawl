using UnityEngine;

public class CharacterParry : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip parrySuccessSound;
    private AudioSource audioSource;

    private PlayerController controller;
    private EnergyManager energy;

    private bool hasParriedThisHit = false;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyManager>();
        audioSource = GetComponent<AudioSource>();
        GameSettings.RegisterSource(audioSource, GameSound.SoundEffects);
    }
    public void TryParry()
    {
        IState currentState = controller.GetCurrentState();

        if (currentState == controller.stateMachine.Idle || currentState == controller.stateMachine.Move)
        {
            hasParriedThisHit = false;
            controller.ChangeState(StateCharacter.Parry);
        }
    }

    public void OnSuccessfulParry()
    {
        if (hasParriedThisHit) return;

        if (energy != null)
        {
            energy.AddEnergy(50);
        }

        if (audioSource != null && parrySuccessSound != null)
        {
            audioSource.PlayOneShot(parrySuccessSound);
        }

        hasParriedThisHit = true;
    }
}
