using UnityEngine;
public class CharacterDodge : MonoBehaviour
{
    [Header("Air Dodge Usage")]
    [SerializeField, Min(1)] private int maxDodges = 3;
    [SerializeField, Min(0f)] private float cooldownDuration = 5f;

    public bool CanDodge => usage != null && usage.CanUse;
    public int RemainingDodges => usage?.RemainingUses ?? 0;
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

    public bool TryStartDodge() => usage.TryConsume();

    public void CompleteDodge() => usage.CompleteUse();

    public void ResetDodges() => usage.Reset();

    private void InitializeUsage()
    {
        usage = new LimitedUseCooldown(Mathf.Max(1, maxDodges), Mathf.Max(0f, cooldownDuration));
    }
}
