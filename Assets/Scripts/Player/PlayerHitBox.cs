using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [Header("Combat Hitboxes")]
    [SerializeField] private AttackHitbox jabHitbox;
    [SerializeField] private AttackHitbox fTiltHitbox;
    [SerializeField] private AttackHitbox upTiltHitbox;
    [SerializeField] private AttackHitbox dTiltHitbox;

    [SerializeField] private bool originalSpriteFacesRight = true;
    public bool IsFacingRight { get; private set; }

    private void Awake()
    {
        IsFacingRight = originalSpriteFacesRight;

        if (transform.localScale.x < 0)
        {
            IsFacingRight = !originalSpriteFacesRight;
        }
    }

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
        if (active) fTiltHitbox?.BeginSwing(); 
        else fTiltHitbox?.EndSwing(); 
    }
    public void SetUTiltHitbox(bool active) 
    { 
        if (active) upTiltHitbox?.BeginSwing(); 
        else upTiltHitbox?.EndSwing(); 
    }
    public void SetDTiltHitbox(bool active)
    {
        if (active) dTiltHitbox?.BeginSwing();
        else dTiltHitbox?.EndSwing();
    }
}
