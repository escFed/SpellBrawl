public interface IHitStunned: IDamageable
{
    void TakeDamage(int amount, UnityEngine.Vector2 knockback, float hitStun);
}
