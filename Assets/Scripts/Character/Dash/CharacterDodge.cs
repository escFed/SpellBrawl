using UnityEngine;

public class CharacterDodge : MonoBehaviour
{
    private const float CooldownDuration = 1f;

    public bool CanDodge => usage != null && usage.CanUse;
    public float CooldownRemaining => usage?.CooldownRemaining ?? 0f;

    private AbilityCooldown usage;

    private void Awake()
    {
        InitializeUsage();
    }

    private void Update()
    {
        usage.Tick(Time.deltaTime);
    }

    public bool TryStartDodge() => usage.TryStart();

    public void CompleteDodge() => usage.CompleteUse();

    public void ResetDodges() => usage.Reset();

    private void InitializeUsage()
    {
        usage = new AbilityCooldown(CooldownDuration);
    }
}
