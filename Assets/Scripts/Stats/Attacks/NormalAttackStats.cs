using UnityEngine;

public abstract class NormalAttackStats : AttackStats
{
    [Header("Hit Data")]
    public int damage = 10;

    public Vector2 knockback = new Vector2(4f, 2f);
}
