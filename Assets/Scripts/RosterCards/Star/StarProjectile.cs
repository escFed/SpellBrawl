using UnityEngine;

public class StarProjectile : MonoBehaviour
{
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 4f };
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [Header("Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(5f, 5f);
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float speed = 7f;
    [Header("Animation")]
    [SerializeField] private Sprite[] travelFrames;
    [SerializeField] private Sprite[] impactFrames;
    [SerializeField, Min(0.01f)] private float frameDuration = 0.1f;

    private GameObject caster;
    private Transform target;
    private int attackerPlayerIndex = -1;
    private SpriteRenderer starSprite;
    private float animationTime;
    private bool isImpactPlaying;

    private void Awake()
    {
        starSprite = GetComponent<SpriteRenderer>();
        if (starSprite != null && travelFrames != null && travelFrames.Length > 0)
            starSprite.sprite = travelFrames[0];
    }

    public void Init(GameObject casterObject, Transform targetTransform)
    {
        caster = casterObject;
        target = targetTransform;
        attackerPlayerIndex = caster != null && caster.TryGetComponent(out CharacterCoordinator controller)
            ? controller.PlayerIndex
            : -1;

        Destroy(gameObject, lifeTime);
        AIDangerSource.Attach(gameObject, caster, 0.55f, target);
    }

    private void Update()
    {
        animationTime += Time.deltaTime;

        if (isImpactPlaying)
        {
            ShowFrame(impactFrames, animationTime);
            return;
        }

        ShowFrame(travelFrames, animationTime);

        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;

            // The travel artwork points diagonally down and right at zero rotation.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 45f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isImpactPlaying) return;

        if (caster != null && collision.transform.root == caster.transform.root) return;

        ICombatHitReceiver hitTarget = collision.GetComponentInParent<ICombatHitReceiver>();
        if (hitTarget != null)
        {
            bool applied = hitTarget.ReceiveHit(new CombatHit(damage, knockback, hitStun, HitReaction.Hit, collision.ClosestPoint(transform.position), attackerPlayerIndex, launch,
                attacker: caster != null ? caster.GetComponent<CharacterCoordinator>() : null));
            if (applied) GetComponent<MatchCardUse>()?.MarkHit();

            if (impactFrames != null && impactFrames.Length > 0)
            {
                isImpactPlaying = true;
                target = null;
                animationTime = 0f;
                transform.rotation = Quaternion.identity;
                GetComponent<Collider2D>().enabled = false;
                ShowFrame(impactFrames, 0f);
                Destroy(gameObject, Mathf.Max(0.01f, frameDuration) * impactFrames.Length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void ShowFrame(Sprite[] frames, float elapsed)
    {
        if (starSprite == null || frames == null || frames.Length == 0) return;

        int frame = Mathf.Min(Mathf.FloorToInt(elapsed / Mathf.Max(0.01f, frameDuration)), frames.Length - 1);
        if (frames[frame] != null)
            starSprite.sprite = frames[frame];
    }
}
