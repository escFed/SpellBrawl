using UnityEngine;

[CreateAssetMenu(fileName = "AttackStats", menuName = "Character/AttackStats")]
public class AttackStats : ScriptableObject
{
    [Header("Frame Data")]
    public float startup = 0.05f;
    public float active = 0.05f;
    public float recovery = 0.15f;
    public float cooldown = 0f;

    [Header("Hit Data")]
    public int damage = 10;
    public Vector2 knockback = new Vector2(4f, 2f);
    public int energyGain = 10;
}
