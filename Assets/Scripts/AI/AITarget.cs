using UnityEngine;

public class AITarget
{
    private CharacterCoordinator selfController;

    public CharacterCoordinator TargetController { get; private set; }
    public CharacterHealth TargetHealth { get; private set; }
    public Transform Target { get; private set; }
    public Vector3 PerceivedTargetPosition { get; private set; }

    public void Initialize(CharacterCoordinator self, Vector3 initialPosition)
    {
        selfController = self;
        PerceivedTargetPosition = initialPosition;
    }

    public void Tick()
    {
        if (TargetController != null &&
            !TargetController.IsDead &&
            TargetController.gameObject.activeInHierarchy)
        {
            return;
        }

        FindTarget();
    }

    public void UpdatePerception()
    {
        if (Target != null)
            PerceivedTargetPosition = Target.position;
    }

    private void FindTarget()
    {
        CharacterCoordinator[] allPlayers = Object.FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);

        TargetController = null;
        TargetHealth = null;
        Target = null;

        foreach (CharacterCoordinator player in allPlayers)
        {
            if (player == selfController || player.IsDead)
                continue;

            TargetController = player;
            TargetHealth = player.GetComponent<CharacterHealth>();
            Target = player.transform;
            PerceivedTargetPosition = Target.position;
            return;
        }
    }
}

