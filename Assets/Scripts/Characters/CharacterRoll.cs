using UnityEngine;

public class CharacterRoll : MonoBehaviour
{
    [Header("Roll Usage")]
    [SerializeField, Min(1)] private int maxRolls = 3;
    [SerializeField, Min(0f)] private float cooldownDuration = 5f;

    public bool CanRoll => usage != null && usage.CanUse;
    public int RemainingRolls => usage?.RemainingUses ?? 0;
    public float CooldownRemaining => usage?.CooldownRemaining ?? 0f;

    private LimitedUseCooldown usage;

    private void Awake()
    {
        InitializeUsage();
    }

    private void Update()
    {
        usage.Tick(Time.deltaTime);
    }

    public bool TryStartRoll() => usage.TryConsume();

    public void CompleteRoll() => usage.CompleteUse();

    public void ResetRolls() => usage.Reset();

    private void InitializeUsage()
    {
        usage = new LimitedUseCooldown(Mathf.Max(1, maxRolls), Mathf.Max(0f, cooldownDuration));
    }
}
