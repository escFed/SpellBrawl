using System.Collections.Generic;
using UnityEngine;

public class TsunamiWave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float pushSpeed = 9f;
    [SerializeField, Min(0f)] private float windUpDuration = 0.25f;
    [SerializeField, Min(0f)] private float travelDuration = 2.5f;

    private readonly HashSet<CharacterMovement> affected = new HashSet<CharacterMovement>();
    private CharacterCoordinator caster;
    private float direction;
    private float activeAt;
    private bool initialized;

    public void Init(CharacterCoordinator owner, float horizontalDirection)
    {
        caster = owner;
        direction = horizontalDirection < 0f ? -1f : 1f;
        activeAt = Time.time + windUpDuration;
        initialized = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.flipX = direction < 0f;

        Destroy(gameObject, windUpDuration + travelDuration);
        AIDangerSource.Attach(gameObject, owner != null ? owner.gameObject : null, 2.2f);
    }

    private void FixedUpdate()
    {
        if (!initialized || Time.time < activeAt)
            return;

        transform.position += Vector3.right * (direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision) => RefreshTarget(collision);
    private void OnTriggerStay2D(Collider2D collision) => RefreshTarget(collision);

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out CharacterMovement movement))
            RemoveTarget(movement);
    }

    private void RefreshTarget(Collider2D collision)
    {
        // Only the character's body collider may register a push, not child hitboxes.
        if (!collision.TryGetComponent(out CharacterCoordinator target) || target == caster ||
            target.Movement == null)
            return;

        CharacterMovement movement = target.Movement;
        if (!initialized || Time.time < activeAt || target.Health == null ||
            target.Health.Phase != RespawnPhase.Active || target.IsIntangible || target.IsParrying ||
            (target.Shield != null && target.Shield.IsActive))
        {
            RemoveTarget(movement);
            return;
        }

        movement.SetExternalHorizontalPush(this, direction * pushSpeed);
        affected.Add(movement);
    }

    private void RemoveTarget(CharacterMovement movement)
    {
        if (movement == null)
            return;

        movement.ClearExternalHorizontalPush(this);
        affected.Remove(movement);
    }

    private void OnDisable()
    {
        foreach (CharacterMovement movement in affected)
        {
            if (movement != null)
                movement.ClearExternalHorizontalPush(this);
        }
        affected.Clear();
    }
}
