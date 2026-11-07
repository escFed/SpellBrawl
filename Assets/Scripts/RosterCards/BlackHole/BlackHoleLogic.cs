using UnityEngine;

public class BlackHoleLogic : MonoBehaviour
{
    [Header("Gravity Field")]
    [SerializeField, Min(0f)] private float effectDuration = 1.75f;
    [SerializeField, Range(0f, 1f)] private float moveSpeedMultiplier = 0.65f;
    [SerializeField, Min(0f)] private float pullSpeed = 0.8f;
    [SerializeField, Min(0f)] private float centerDeadZone = 0.25f;

    private Collider2D fieldCollider;
    private Collider2D targetCollider;
    private CharacterCoordinator targetController;
    private CharacterMovement targetMovement;
    private float expiresAt;

    private void Awake()
    {
        fieldCollider = GetComponent<Collider2D>();
    }

    public void Initialize(CharacterCoordinator target)
    {
        targetController = target;
        targetMovement = target != null ? target.Movement : null;
        targetCollider = target != null ? target.GetComponent<CapsuleCollider2D>() : null;
        expiresAt = Time.time + effectDuration;
        Destroy(gameObject, effectDuration);
    }

    private void FixedUpdate()
    {
        if (Time.time >= expiresAt || fieldCollider == null || targetController == null ||
            targetMovement == null || targetCollider == null || !targetCollider.enabled ||
            targetController.Health == null || targetController.Health.Phase != RespawnPhase.Active ||
            targetController.IsIntangible || targetController.IsParrying ||
            (targetController.Shield != null && targetController.Shield.IsActive) ||
            !fieldCollider.Distance(targetCollider).isOverlapped)
        {
            ClearEffect();
            return;
        }

        targetMovement.SetExternalMoveSpeedMultiplier(this, moveSpeedMultiplier);

        float offset = transform.position.x - targetController.transform.position.x;
        if (Mathf.Abs(offset) > centerDeadZone)
            targetMovement.SetExternalHorizontalPush(this, Mathf.Sign(offset) * pullSpeed);
        else
            targetMovement.ClearExternalHorizontalPush(this);
    }

    private void OnDisable()
    {
        ClearEffect();
    }

    private void ClearEffect()
    {
        if (targetMovement == null)
            return;

        targetMovement.ClearExternalMoveSpeedMultiplier(this);
        targetMovement.ClearExternalHorizontalPush(this);
    }
}
