using UnityEngine;

public class CharacterParry : MonoBehaviour
{
    private PlayerController controller;
    private EnergyManager energy;

    private bool hasParriedThisHit = false;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyManager>();
    }
    public void TryParry()
    {
        IState currentState = controller.GetCurrentState();

        if (currentState == controller.stateMachine.Idle ||
            currentState == controller.stateMachine.Move ||
            currentState == controller.stateMachine.Shield)
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

        hasParriedThisHit = true;
    }
}
