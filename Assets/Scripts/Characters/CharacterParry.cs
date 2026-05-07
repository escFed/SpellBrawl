using UnityEngine;

public class CharacterParry : MonoBehaviour
{
    private PlayerController controller;
    private EnergyManager energy;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyManager>();
    }
    public void TryParry()
    {
        IState currentState = controller.GetCurrentState();

        if (currentState == controller.stateMachine.Idle || currentState == controller.stateMachine.Move)
        {
            controller.ChangeState(StateCharacter.Parry);
        }
    }

    public void OnSuccessfulParry()
    {
        if (energy != null)
        {
            energy.AddEnergy(50);
        }

        controller.ChangeState(StateCharacter.Idle);
    }
}
