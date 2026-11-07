using UnityEngine;

public class DragonBreathLogic : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float windUpDuration = 0.25f;
    [SerializeField, Min(0f)] private float breathDuration = 2f;
    [SerializeField, Min(0.01f)] private float damageInterval = 0.5f;

    [Header("Flame")]
    [SerializeField, Min(0f)] private float range = 3f;
    [SerializeField, Min(0f)] private float nearHalfHeight = 0.4f;
    [SerializeField, Min(0f)] private float farHalfHeight = 1f;
    [SerializeField, Min(0)] private int burnDamage = 2;
    [SerializeField, Min(0)] private int finisherDamage = 4;
    [SerializeField] private Vector2 finisherKnockback = new Vector2(5f, 1.5f);
    [SerializeField] private KnockbackProfile finisherLaunch = new KnockbackProfile { growth = 3f };
    [SerializeField, Min(0f)] private float finisherHitStun = 0.25f;

    [Header("Presentation")]
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite[] flameFrames;
    [SerializeField, Min(1f)] private float animationFramesPerSecond = 10f;
    [SerializeField] private AudioClip breathClip;
    [SerializeField] private AudioSource s;

    private const int MaximumBurnTicks = 4;
    private CharacterCoordinator caster;
    private CharacterCoordinator target;
    private Collider2D targetBody;
    private CharacterHitBox casterHitBox;
    private SpriteRenderer flameSprite;
    private Color flameColor;
    private float authoredSoundVolume;
    private float direction;
    private float startedAt;
    private float nextDamageAt;
    private int consecutiveTicks;
    private bool flameStarted;
    private bool initialized;
    private int currentFrame = -1;

    public float TotalDuration => windUpDuration + breathDuration;

    public void Initialize(CharacterCoordinator owner, float facingDirection)
    {
        caster = owner;
        direction = facingDirection < 0f ? -1f : 1f;
        startedAt = Time.time;
        initialized = true;
        casterHitBox = owner != null ? owner.GetComponent<CharacterHitBox>() : null;
        casterHitBox?.LockFacing();

        CharacterCoordinator[] players = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator player in players)
        {
            if (player == owner || player.Health == null)
                continue;

            target = player;
            targetBody = player.GetComponent<Collider2D>();
            break;
        }

        flameSprite = GetComponent<SpriteRenderer>();
        if (flameSprite != null)
        {
            flameColor = flameSprite.color;
            flameSprite.flipX = direction < 0f; // The sprite widens from left to right.
            SetSprite(warningSprite != null ? warningSprite : flameSprite.sprite);
            SetFlameColor(new Color(1f, 0.55f, 0.2f, 0.4f));
        }

        if (s != null && breathClip != null)
        {
            authoredSoundVolume = s.volume;
            s.clip = breathClip;
            s.loop = false;
            s.volume = 0f;
            s.Play();
        }

        UpdatePosition();
        AIDangerSource.Attach(gameObject, owner != null ? owner.gameObject : null, range * 0.6f);
    }

    private void Update()
    {
        if (!initialized || caster == null || caster.IsDead || caster.GetCurrentState() != caster.States.Card)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePosition();
        float elapsed = Time.time - startedAt;
        UpdateSound(elapsed);
        if (elapsed < windUpDuration)
            return;

        if (!flameStarted)
        {
            flameStarted = true;
            nextDamageAt = Time.time;
            SetFlameColor(flameColor);
        }

        UpdateAnimation(elapsed - windUpDuration);

        bool inFlame = TargetInFlame();
        if (!inFlame)
        {
            consecutiveTicks = 0;
            nextDamageAt = Time.time;
        }
        else if (elapsed < TotalDuration && consecutiveTicks < MaximumBurnTicks && Time.time >= nextDamageAt)
        {
            ApplyBurn();
            consecutiveTicks++;
            nextDamageAt = Time.time + damageInterval;
        }

        if (elapsed < TotalDuration)
            return;

        if (inFlame && consecutiveTicks == MaximumBurnTicks)
            ApplyFinisher();
        Destroy(gameObject);
    }

    private bool TargetInFlame()
    {
        return target != null && targetBody != null && targetBody.enabled &&
            target.Health != null && target.Health.Phase == RespawnPhase.Active &&
            !target.IsIntangible && !target.Health.IsDamageImmune &&
            IsTargetInCone(FlameOrigin(), direction, range, nearHalfHeight, farHalfHeight, targetBody.bounds);
    }

    private void ApplyBurn()
    {
        Vector2 point = targetBody.ClosestPoint(FlameOrigin());
        bool applied = target.Health.ReceiveHit(new CombatHit(burnDamage, Vector2.zero, 0f,
            HitReaction.Hit, point, caster.PlayerIndex, growthOverride: 0f, attacker: caster));
        if (applied)
            GetComponent<MatchCardUse>()?.MarkHit();

        if (target.IsParrying)
            Destroy(gameObject);
    }

    private void ApplyFinisher()
    {
        Vector2 point = targetBody.ClosestPoint(FlameOrigin());
        Vector2 knockback = new Vector2(finisherKnockback.x * direction, finisherKnockback.y);
        bool applied = target.Health.ReceiveHit(new CombatHit(finisherDamage, knockback,
            finisherHitStun, HitReaction.Hit, point, caster.PlayerIndex, finisherLaunch, attacker: caster));
        if (applied)
            GetComponent<MatchCardUse>()?.MarkHit();
    }

    private Vector2 FlameOrigin() => (Vector2)caster.transform.position + new Vector2(direction * 0.25f, 0.1f);

    private void UpdatePosition()
    {
        Vector2 origin = FlameOrigin();
        transform.position = origin + Vector2.right * (direction * range * 0.5f);
    }

    private void UpdateAnimation(float activeTime)
    {
        if (flameFrames == null || flameFrames.Length == 0)
            return;

        int frame = Mathf.FloorToInt(activeTime * animationFramesPerSecond) % flameFrames.Length;
        if (frame == currentFrame)
            return;

        currentFrame = frame;
        SetSprite(flameFrames[frame]);
    }

    private void SetSprite(Sprite sprite)
    {
        if (flameSprite == null || sprite == null)
            return;

        flameSprite.sprite = sprite;
        Vector2 spriteSize = sprite.bounds.size;
        transform.localScale = new Vector3(
            range / Mathf.Max(spriteSize.x, 0.01f),
            farHalfHeight * 2f / Mathf.Max(spriteSize.y, 0.01f), 1f);
    }

    private void SetFlameColor(Color color)
    {
        if (flameSprite != null)
            flameSprite.color = color;
    }

    private void UpdateSound(float elapsed)
    {
        if (s == null || breathClip == null)
            return;

        float level = elapsed < windUpDuration
            ? Mathf.Lerp(0.35f, 0.65f, elapsed / Mathf.Max(windUpDuration, 0.01f))
            : Mathf.Lerp(0.65f, 1f, Mathf.Clamp01((elapsed - windUpDuration) / 0.15f));
        level *= Mathf.Clamp01((TotalDuration - elapsed) / 0.2f);
        s.volume = authoredSoundVolume * GameSettings.SoundEffectsVolume * level;
    }

    private void OnDestroy()
    {
        if (casterHitBox != null)
            casterHitBox.UnlockFacing();
    }

    public static bool IsTargetInCone(Vector2 origin, float facingDirection, float length,
        float nearHeight, float farHeight, Bounds targetBounds)
    {
        float forward = (targetBounds.center.x - origin.x) * (facingDirection < 0f ? -1f : 1f);
        if (forward + targetBounds.extents.x < 0f || forward - targetBounds.extents.x > length)
            return false;

        float t = Mathf.Clamp01(forward / Mathf.Max(length, 0.01f));
        float halfHeight = Mathf.Lerp(nearHeight, farHeight, t);
        return Mathf.Abs(targetBounds.center.y - origin.y) <= halfHeight + targetBounds.extents.y;
    }
}
