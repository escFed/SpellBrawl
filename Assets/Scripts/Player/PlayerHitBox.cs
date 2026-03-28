using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [Header("Combat Hitboxes")]
    [SerializeField] private AttackHitbox jabHitbox;
    [SerializeField] private AttackHitbox ftiltHitbox;
    [SerializeField] private AttackHitbox uptiltHitbox;

    public bool IsFacingRight { get; private set; } = true;

    public void CheckAndFlip(float directionX)
    {
        if (directionX > 0 && !IsFacingRight)
            Flip();
        else if (directionX < 0 && IsFacingRight)
            Flip();
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void SetJabHitbox(bool active) 
    { 
        if (active) jabHitbox?.BeginSwing(); 
        else jabHitbox?.EndSwing();
    }
    public void SetFTiltHitbox(bool active) 
    { 
        if (active) ftiltHitbox?.BeginSwing(); 
        else ftiltHitbox?.EndSwing(); 
    }
    public void SetUTiltHitbox(bool active) 
    { 
        if (active) uptiltHitbox?.BeginSwing(); 
        else uptiltHitbox?.EndSwing(); 
    }
}
