using UnityEngine;

[CreateAssetMenu(fileName = "ThrowStats", menuName = "Character/ThrowStats")]
public class ThrowStats : ScriptableObject
{
    [Header("Frame Data")]
    public float releaseDelay = 0.12f;
    public float recovery = 0.25f;

    [Header("Hit Data")]
    public int damage = 8;
    public Vector2 knockback = new Vector2(6f, 3f);
    public int energyGain = 10;
}
