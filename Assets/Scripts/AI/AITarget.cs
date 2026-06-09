using UnityEngine;

public class AITarget
{
    private PlayerController selfController;

    public PlayerController TargetController { get; private set; }
    public CharacterHealth TargetHealth { get; private set; }
    public Transform Target { get; private set; }
    public Vector3 PerceivedTargetPosition { get; private set; }

    public void Initialize(PlayerController self, Vector3 initialPosition)
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
        PlayerController[] allPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        TargetController = null;
        TargetHealth = null;
        Target = null;

        foreach (PlayerController player in allPlayers)
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

