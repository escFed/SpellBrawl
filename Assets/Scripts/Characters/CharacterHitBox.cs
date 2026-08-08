using UnityEngine;

public class CharacterHitBox : MonoBehaviour
{
    [Header("Combat Hitboxes")]
    [SerializeField] private AttackHitbox jabHitbox;
    [SerializeField] private AttackHitbox fTiltHitbox;
    [SerializeField] private AttackHitbox upTiltHitbox;
    [SerializeField] private AttackHitbox dTiltHitbox;
    [SerializeField] private AttackHitbox neutralAirHitbox;
    [SerializeField] private AttackHitbox forwardAirHitbox;
    [SerializeField] private AttackHitbox upAirHitbox;
    [SerializeField] private AttackHitbox downAirHitbox;
    [SerializeField] private AttackHitbox forwardHeavyHitbox;
    [SerializeField] private AttackHitbox upHeavyHitbox;
    [SerializeField] private AttackHitbox downHeavyHitbox;
    [SerializeField] private GrabHitbox grabHitbox;
    [SerializeField] private GrabHitbox pivotGrabHitbox;

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

    public void FaceDirection(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f)
            return;

        IsFacingRight = directionX > 0f;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (IsFacingRight ? 1f : -1f);
        transform.localScale = localScale;
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void SetupJab(GroundAttackStats stats)
    {
        if (jabHitbox == null) { Debug.LogError($"[CharacterHitBox] jabHitbox no está asignado en el Inspector del objeto '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] jabAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        jabHitbox.Setup(stats);
    }
    public void SetupFTilt(GroundAttackStats stats)
    {
        if (fTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] fTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] fTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        fTiltHitbox.Setup(stats);
    }
    public void SetupUTilt(GroundAttackStats stats)
    {
        if (upTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] upTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] upTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        upTiltHitbox.Setup(stats);
    }
    public void SetupDTilt(GroundAttackStats stats)
    {
        if (dTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] dTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] dTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        dTiltHitbox.Setup(stats);
    }
    public void SetupNeutralAir(AerialAttackStats stats) => SetupHitbox(GetNeutralAirHitbox(), stats, "neutralAirHitbox", "neutralAirAttack");
    public void SetupForwardAir(AerialAttackStats stats) => SetupHitbox(GetForwardAirHitbox(), stats, "forwardAirHitbox", "forwardAirAttack");
    public void SetupUpAir(AerialAttackStats stats) => SetupHitbox(GetUpAirHitbox(), stats, "upAirHitbox", "upAirAttack");
    public void SetupDownAir(AerialAttackStats stats) => SetupHitbox(GetDownAirHitbox(), stats, "downAirHitbox", "downAirAttack");
    public void SetupHeavyAttack(HeavyAttackType type, HeavyAttackStats stats, int damage, Vector2 knockback)
    {
        AttackHitbox attackHitbox = GetHeavyHitbox(type);
        if (attackHitbox == null)
        {
            Debug.LogError($"[CharacterHitBox] {type} Heavy hitbox no esta asignado en el Inspector de '{gameObject.name}'");
            return;
        }

        attackHitbox.Setup(stats, damage, knockback);
    }
    public void SetupGrabbox(CharacterGrab grab) => SetupGrabbox(GetGrabbox(), grab, "grabHitbox");
    public void SetupPivotGrabbox(CharacterGrab grab) => SetupGrabbox(GetPivotGrabbox(), grab, "pivotGrabHitbox");

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
    public void SetNeutralAirHitbox(bool active) => SetHitboxActive(GetNeutralAirHitbox(), active);
    public void SetForwardAirHitbox(bool active) => SetHitboxActive(GetForwardAirHitbox(), active);
    public void SetUpAirHitbox(bool active) => SetHitboxActive(GetUpAirHitbox(), active);
    public void SetDownAirHitbox(bool active) => SetHitboxActive(GetDownAirHitbox(), active);
    public void SetHeavyHitbox(HeavyAttackType type, bool active) => SetHitboxActive(GetHeavyHitbox(type), active);
    public void CloseAllHeavyHitboxes()
    {
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Forward), false);
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Up), false);
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Down), false);
    }
    public void SetGrabbox(bool active) => SetGrabboxActive(GetGrabbox(), active);
    public void SetPivotGrabbox(bool active) => SetGrabboxActive(GetPivotGrabbox(), active);

    private AttackHitbox GetNeutralAirHitbox() => neutralAirHitbox != null ? neutralAirHitbox : fTiltHitbox;
    private AttackHitbox GetForwardAirHitbox() => forwardAirHitbox != null ? forwardAirHitbox : fTiltHitbox;
    private AttackHitbox GetUpAirHitbox() => upAirHitbox != null ? upAirHitbox : upTiltHitbox;
    private AttackHitbox GetDownAirHitbox() => downAirHitbox != null ? downAirHitbox : dTiltHitbox;
    private AttackHitbox GetHeavyHitbox(HeavyAttackType type)
    {
        return type switch
        {
            HeavyAttackType.Up => upHeavyHitbox != null ? upHeavyHitbox : upTiltHitbox,
            HeavyAttackType.Down => downHeavyHitbox != null ? downHeavyHitbox : dTiltHitbox,
            _ => forwardHeavyHitbox != null ? forwardHeavyHitbox : fTiltHitbox
        };
    }
    private GrabHitbox GetGrabbox() => grabHitbox;
    private GrabHitbox GetPivotGrabbox() => pivotGrabHitbox != null ? pivotGrabHitbox : grabHitbox;

    private void SetupHitbox(AttackHitbox attackHitbox, NormalAttackStats stats, string hitboxName, string statsName)
    {
        if (attackHitbox == null) { Debug.LogError($"[CharacterHitBox] {hitboxName} no esta asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] {statsName} stats es null en CharacterStats de '{gameObject.name}'"); return; }
        attackHitbox.Setup(stats);
    }

    private void SetHitboxActive(AttackHitbox attackHitbox, bool active)
    {
        if (active) attackHitbox?.BeginSwing();
        else attackHitbox?.EndSwing();
    }

    private void SetupGrabbox(GrabHitbox grabbox, CharacterGrab grab, string grabboxName)
    {
        if (grabbox == null) { Debug.LogError($"[CharacterHitBox] {grabboxName} no esta asignado en el Inspector de '{gameObject.name}'"); return; }
        grabbox.Setup(grab);
    }

    private void SetGrabboxActive(GrabHitbox grabbox, bool active)
    {
        if (active) grabbox?.BeginGrab();
        else grabbox?.EndGrab();
    }
}
