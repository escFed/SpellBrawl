using UnityEngine;

public class CharacterHitBox : MonoBehaviour
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

    public void SetupJab(AttackStats stats)
    {
        if (jabHitbox == null) { Debug.LogError($"[CharacterHitBox] jabHitbox no está asignado en el Inspector del objeto '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] jabAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        jabHitbox.Setup(stats);
    }
    public void SetupFTilt(AttackStats stats)
    {
        if (fTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] fTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] fTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        fTiltHitbox.Setup(stats);
    }
    public void SetupUTilt(AttackStats stats)
    {
        if (upTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] upTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] upTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        upTiltHitbox.Setup(stats);
    }
    public void SetupDTilt(AttackStats stats)
    {
        if (dTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] dTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] dTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        dTiltHitbox.Setup(stats);
    }

    public void SetJabHitbox(bool active)
    {
        if (jabHitbox == null) return;
        if (active) jabHitbox.BeginSwing();
        else jabHitbox.EndSwing();
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
