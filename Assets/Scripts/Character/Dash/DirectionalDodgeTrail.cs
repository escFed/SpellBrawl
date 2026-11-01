using UnityEngine;

public sealed class DirectionalDodgeTrail : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trailRenderer;
    [SerializeField] private Sprite[] frames;

    private Vector2 worldDirection = Vector2.right;
    private float elapsed;
    private float duration;
    private bool playing;

    private void Awake() => Stop();

    private void OnDisable() => Stop();

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
            worldDirection = direction.normalized;
        OrientTrail();
    }

    public void Play(float activeDuration)
    {
        if (trailRenderer == null || frames == null || frames.Length == 0)
            return;

        elapsed = 0f;
        duration = Mathf.Max(0.01f, activeDuration);
        playing = true;
        OrientTrail();
        trailRenderer.sprite = frames[0];
        trailRenderer.enabled = true;
    }

    public void Stop()
    {
        playing = false;
        if (trailRenderer != null)
            trailRenderer.enabled = false;
    }

    private void Update()
    {
        if (!playing)
            return;

        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Stop();
            return;
        }

        OrientTrail();
        int frame = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(elapsed / duration * frames.Length));
        trailRenderer.sprite = frames[frame];
    }

    private void OrientTrail()
    {
        if (trailRenderer == null)
            return;

        // The character flips by negating its root X scale. Undo that flip
        // before rotating a right-facing trail toward the world direction.
        float facing = transform.lossyScale.x < 0f ? -1f : 1f;
        Vector2 localDirection = new Vector2(worldDirection.x * facing, worldDirection.y);
        float angle = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
        trailRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
