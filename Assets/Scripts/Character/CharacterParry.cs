using UnityEngine;

public class CharacterParry : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip parrySuccessSound;
    private AudioSource audioSource;

    private CharacterCoordinator controller;
    private EnergyManager energy;

    private bool hasParriedThisHit = false;
    public bool IsParrying { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        energy = GetComponent<EnergyManager>();
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
