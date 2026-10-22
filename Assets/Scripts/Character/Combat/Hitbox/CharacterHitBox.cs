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
    [SerializeField] private AttackHitbox diagonalDownAirHitbox;
    [SerializeField] private AttackHitbox forwardHeavyHitbox;
    [SerializeField] private AttackHitbox upHeavyHitbox;
    [SerializeField] private AttackHitbox downHeavyHitbox;
    [SerializeField] private GrabHitbox grabHitbox;
    [SerializeField] private GrabHitbox pivotGrabHitbox;

    [SerializeField] private bool originalSpriteFacesRight = true;
    public bool IsFacingRight { get; private set; }
    private bool facingLocked;

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
        if (facingLocked)
            return;
        if (directionX > 0 && !IsFacingRight)
            Flip();
        else if (directionX < 0 && IsFacingRight)
            Flip();
    }

    public void FaceDirection(float directionX)
    {
        if (facingLocked)
            return;
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

    public bool HasDiagonalDownAirHitbox => diagonalDownAirHitbox != null;
    public void LockFacing() => facingLocked = true;
    public void UnlockFacing() => facingLocked = false;

    private void OnDisable() => facingLocked = false;

    public void SetupJab(GroundAttackStats stats)
    {
        if (jabHitbox == null) { Debug.LogError($"[CharacterHitBox] jabHitbox no está asignado en el Inspector del objeto '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] jabAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        jabHitbox.Setup(stats);
    }
    public void SetupJabStage(GroundAttackStats stats, int stage)
    {
        if (jabHitbox == null) { Debug.LogError($"[CharacterHitBox] jabHitbox no está asignado en el Inspector del objeto '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] jabAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }

        if (stage >= 2)
        {
            jabHitbox.Setup(stats);
            return;
        }

        // The first two contacts hold the defender close; only the finisher launches.
        int damage = Mathf.Max(1, Mathf.CeilToInt(stats.damage * 0.5f));
        Vector2 knockback = new Vector2(Mathf.Max(0.4f, stats.knockback.x * 0.25f), 0f);
        float hitStun = Mathf.Max(0.24f, stats.hitStun);
        jabHitbox.Setup(stats, damage, knockback, hitStun, 0f);
    }
    public void SetupFTilt(GroundAttackStats stats)
    {
        if (fTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] fTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] fTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }
        fTiltHitbox.Setup(stats);
    }
    public void SetupForwardTiltStage(GroundAttackStats stats, int stage)
    {
        if (fTiltHitbox == null) { Debug.LogError($"[CharacterHitBox] fTiltHitbox no está asignado en el Inspector de '{gameObject.name}'"); return; }
        if (stats == null) { Debug.LogError($"[CharacterHitBox] fTiltAttack stats es null en CharacterStats de '{gameObject.name}'"); return; }

        if (stage == 0)
        {
            int damage = Mathf.Max(1, Mathf.RoundToInt(stats.damage * 0.375f));
            Vector2 knockback = new Vector2(Mathf.Max(0.5f, stats.knockback.x * 0.15f), 0f);
            fTiltHitbox.Setup(stats, damage, knockback, Mathf.Max(0.3f, stats.hitStun), 0f);
        }
        else
        {
            int damage = Mathf.Max(1, stats.damage - Mathf.Max(1, Mathf.RoundToInt(stats.damage * 0.375f)));
            fTiltHitbox.Setup(stats, damage, stats.knockback);
        }
    }
    public bool IsForwardTiltHitbox(AttackHitbox source) => source == fTiltHitbox;
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
    public void SetupDiagonalDownAir(AerialAttackStats stats) => SetupHitbox(diagonalDownAirHitbox, stats, "diagonalDownAirHitbox", "diagonalDownAirAttack");
    public void SetupHeavyAttack(HeavyAttackType type, HeavyAttackStats stats, int damage, Vector2 knockback, float hitStun, float growthOverride = -1f)
    {
        AttackHitbox attackHitbox = GetHeavyHitbox(type);
        if (attackHitbox == null)
        {
            Debug.LogError($"[CharacterHitBox] {type} Heavy hitbox no esta asignado en el Inspector de '{gameObject.name}'");
            return;
        }

        attackHitbox.Setup(stats, damage, knockback, hitStun, growthOverride);
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
    public void SetDiagonalDownAirHitbox(bool active) => SetHitboxActive(diagonalDownAirHitbox, active);
    public void SetHeavyHitbox(HeavyAttackType type, bool active) => SetHitboxActive(GetHeavyHitbox(type), active);
    public void CloseAllHeavyHitboxes()
    {
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Forward), false);
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Up), false);
        SetHitboxActive(GetHeavyHitbox(HeavyAttackType.Down), false);
    }
    public void CloseAllAttackHitboxes()
    {
        SetHitboxActive(jabHitbox, false);
        SetHitboxActive(fTiltHitbox, false);
        SetHitboxActive(upTiltHitbox, false);
        SetHitboxActive(dTiltHitbox, false);
        SetHitboxActive(neutralAirHitbox, false);
        SetHitboxActive(forwardAirHitbox, false);
        SetHitboxActive(upAirHitbox, false);
        SetHitboxActive(downAirHitbox, false);
        SetHitboxActive(diagonalDownAirHitbox, false);
        CloseAllHeavyHitboxes();
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
