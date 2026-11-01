using UnityEngine;

/// <summary>Presentation for Amiro's rune shield and successful parry.</summary>
public sealed class AmiroGuardVisuals : MonoBehaviour
{
    private static readonly int ShieldEnter = Animator.StringToHash("Base Layer.ShieldRuneEnter");
    private static readonly int ShieldHold = Animator.StringToHash("Base Layer.ShieldRuneHold");
    private static readonly int ShieldHit = Animator.StringToHash("Base Layer.ShieldRuneHit");
    private static readonly int ShieldWeak = Animator.StringToHash("Base Layer.ShieldRuneWeak");
    private static readonly int ParrySuccess = Animator.StringToHash("Base Layer.ParryRuneSuccess");

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer shieldRenderer;
    [SerializeField] private CircleCollider2D shieldCollider;
    [SerializeField] private Sprite[] shieldFrames;
    [SerializeField] private SpriteRenderer parryRenderer;
    [SerializeField] private Sprite[] parrySuccessFrames;

    private enum ShieldPhase { Hidden, Opening, Holding, Impact, Breaking }
    private ShieldPhase shieldPhase;
    private float shieldTimer;
    private float parryTimer = -1f;
    private bool shieldWeak;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        HideAll();
    }

    private void Update()
    {
        if (shieldPhase != ShieldPhase.Hidden)
        {
            shieldTimer += Time.deltaTime;
            switch (shieldPhase)
            {
                case ShieldPhase.Opening:
                    if (shieldTimer >= 0.12f) SetShieldHold();
                    break;
                case ShieldPhase.Impact:
                    if (shieldTimer >= 0.10f) SetShieldHold();
                    break;
                case ShieldPhase.Breaking:
                    if (shieldTimer >= 0.08f) SetShieldFrame(5);
                    if (shieldTimer >= 0.18f) HideShield();
                    break;
            }
        }

        if (parryTimer >= 0f)
        {
            parryTimer += Time.deltaTime;
            int index = Mathf.FloorToInt(parryTimer / 0.05f);
            if (parrySuccessFrames == null || index >= parrySuccessFrames.Length)
            {
                parryTimer = -1f;
                if (parryRenderer != null) parryRenderer.enabled = false;
            }
            else if (parryRenderer != null)
            {
                parryRenderer.sprite = parrySuccessFrames[index];
            }
        }
    }

    public void OnShieldStarted()
    {
        shieldWeak = false;
        shieldPhase = ShieldPhase.Opening;
        shieldTimer = 0f;
        if (shieldRenderer != null) shieldRenderer.transform.localRotation = Quaternion.identity;
        if (shieldCollider != null) shieldCollider.enabled = true;
        SetShieldFrame(0);
        if (animator != null) animator.Play(ShieldEnter, 0, 0f);
    }

    public void OnShieldImpact(float remainingFraction, Vector2 hitPoint)
    {
        if (shieldPhase == ShieldPhase.Hidden || shieldPhase == ShieldPhase.Breaking) return;
        shieldWeak = remainingFraction <= 0.4f;
        shieldPhase = ShieldPhase.Impact;
        shieldTimer = 0f;
        if (shieldRenderer != null)
        {
            Vector3 localHit = transform.InverseTransformPoint(hitPoint);
            if (localHit.sqrMagnitude > 0.0001f)
                shieldRenderer.transform.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(localHit.y, localHit.x) * Mathf.Rad2Deg);
        }
        SetShieldFrame(3);
        if (animator != null) animator.Play(ShieldHit, 0, 0f);
    }

    public void OnShieldBroken()
    {
        shieldPhase = ShieldPhase.Breaking;
        shieldTimer = 0f;
        if (shieldCollider != null) shieldCollider.enabled = false;
        if (shieldRenderer != null) shieldRenderer.transform.localRotation = Quaternion.identity;
        SetShieldFrame(4);
    }

    public void OnShieldStopped()
    {
        if (shieldPhase != ShieldPhase.Breaking) HideShield();
    }

    public void OnParrySucceeded()
    {
        parryTimer = 0f;
        if (parryRenderer != null && parrySuccessFrames != null && parrySuccessFrames.Length > 0)
        {
            parryRenderer.sprite = parrySuccessFrames[0];
            parryRenderer.enabled = true;
        }
        if (animator != null) animator.Play(ParrySuccess, 0, 0f);
    }

    private void SetShieldHold()
    {
        shieldPhase = ShieldPhase.Holding;
        shieldTimer = 0f;
        if (shieldRenderer != null) shieldRenderer.transform.localRotation = Quaternion.identity;
        SetShieldFrame(shieldWeak ? 2 : 1);
        if (animator != null) animator.Play(shieldWeak ? ShieldWeak : ShieldHold, 0, 0f);
    }

    private void SetShieldFrame(int index)
    {
        if (shieldRenderer == null || shieldFrames == null || index >= shieldFrames.Length) return;
        shieldRenderer.sprite = shieldFrames[index];
        shieldRenderer.enabled = true;
    }

    private void HideShield()
    {
        shieldPhase = ShieldPhase.Hidden;
        if (shieldRenderer != null) shieldRenderer.enabled = false;
        if (shieldCollider != null) shieldCollider.enabled = false;
    }

    private void HideAll()
    {
        HideShield();
        parryTimer = -1f;
        if (parryRenderer != null) parryRenderer.enabled = false;
    }

    private void OnDisable() => HideAll();
}
