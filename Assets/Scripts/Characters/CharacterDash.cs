using UnityEngine;

public class CharacterDash : MonoBehaviour
{
    public bool CanDash => usage != null && usage.CanUse;
    public float CooldownRemaining => usage?.CooldownRemaining ?? 0f;
    public float Direction { get; private set; } = 1f;

    private LimitedUseCooldown usage;
    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        float cooldown = controller.stats != null ? controller.stats.dashCooldown : 3f;
        usage = new LimitedUseCooldown(1, Mathf.Max(0f, cooldown));
    }

    private void Update()
    {
        usage.Tick(Time.deltaTime);
    }

    public bool TryStartDash(float horizontalDirection)
    {
        if (controller == null || !controller.IsGrounded || Mathf.Abs(horizontalDirection) < controller.stats.tiltThreshold || !usage.TryConsume())
            return false;

        Direction = Mathf.Sign(horizontalDirection);
        controller.Combat.FaceDirection(Direction);

        usage.CompleteUse();
        return true;
    }

    public void ResetDash()
    {
        usage?.Reset();
    }
}
