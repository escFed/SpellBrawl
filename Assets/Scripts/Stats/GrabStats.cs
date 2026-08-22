using UnityEngine;

[CreateAssetMenu(fileName = "GrabStats", menuName = "Character/GrabStats")]
public class GrabStats : ScriptableObject
{
    [Header("Frame Data")]
    public float startup = 0.12f;
    public float active = 0.08f;
    public float recovery = 0.28f;

    [Header("Pummel")]
    public int pummelDamage = 2;
    public float pummelCooldown = 0.35f;
    public int pummelEnergyGain = 2;
    public float maxHoldDuration = 2f;
}