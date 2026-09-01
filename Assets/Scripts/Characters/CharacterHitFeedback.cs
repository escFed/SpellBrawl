using System.Collections;
using UnityEngine;

public class CharacterHitFeedback : MonoBehaviour
{
    private static readonly Color HitFlashColor = new Color(1f, 0.55f, 0.35f, 1f);
    private static readonly Color StrongFlashColor = new Color(1f, 0.3f, 0.15f, 1f);
    private static readonly Color StunnedFlashColor = new Color(0.45f, 0.8f, 1f, 1f);

    private SpriteRenderer sprite;
    private Coroutine flashRoutine;
    private Color colorBeforeFlash = Color.white;

    public void Initialize(SpriteRenderer targetSprite)
    {
        sprite = targetSprite;
    }

    public void Flash(HitReaction reaction)
    {
        if (sprite == null)
            return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            sprite.color = colorBeforeFlash;
        }

        colorBeforeFlash = sprite.color;
        flashRoutine = StartCoroutine(FlashRoutine(reaction));
    }

    private IEnumerator FlashRoutine(HitReaction reaction)
    {
        float duration = reaction switch
        {
            HitReaction.StrongHit => 0.12f,
            HitReaction.Stunned => 0.16f,
            _ => 0.08f
        };
        Color flashColor = reaction switch
        {
            HitReaction.StrongHit => StrongFlashColor,
            HitReaction.Stunned => StunnedFlashColor,
            _ => HitFlashColor
        };

        sprite.color = flashColor;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            sprite.color = Color.Lerp(flashColor, colorBeforeFlash, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        sprite.color = colorBeforeFlash;
        flashRoutine = null;
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (sprite != null)
            sprite.color = colorBeforeFlash;
    }
}
