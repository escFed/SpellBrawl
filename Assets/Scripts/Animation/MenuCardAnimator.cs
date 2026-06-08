using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MenuCardAnimator : MonoBehaviour
{
    public static MenuCardAnimator Instance;

    [Header("Animation Settings")]
    public Canvas canvas;
    public RectTransform deckTarget;
    public float travelDuration = 0.35f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AnimateCardToDeck(RectTransform originalCard, Sprite cardSprite)
    {
        StartCoroutine(MoveFakeCardRoutine(originalCard, deckTarget.position, cardSprite, true));
    }

    public void AnimateCardFromDeck(RectTransform originalCard, Sprite cardSprite)
    {
        StartCoroutine(MoveFakeCardRoutine(deckTarget, originalCard.position, cardSprite, false));
    }

    private IEnumerator MoveFakeCardRoutine(RectTransform startRect, Vector3 endPosition, Sprite sprite, bool shrinkAtEnd)
    {
        GameObject fakeCard = new GameObject("FakeCard_Anim");
        fakeCard.transform.SetParent(canvas.transform);

        Image image = fakeCard.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform fakeRect = fakeCard.GetComponent<RectTransform>();
        fakeRect.sizeDelta = startRect.sizeDelta;
        fakeRect.position = startRect.position;
        fakeRect.localScale = shrinkAtEnd ? Vector3.one : Vector3.zero;

        Vector3 start = fakeRect.position;
        Vector3 middle = (start + endPosition) / 2f;
        middle.y += 120f;

        float elapsed = 0f;

        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / travelDuration;
            t = 1f - Mathf.Pow(1f - t, 3);

            fakeRect.position = Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * middle + Mathf.Pow(t, 2) * endPosition;

            fakeRect.localScale = shrinkAtEnd ? Vector3.Lerp(Vector3.one, Vector3.zero, t) : Vector3.Lerp(Vector3.zero, Vector3.one, t);
            fakeRect.rotation = Quaternion.Euler(0, 0, shrinkAtEnd ? Mathf.Lerp(0, 25f, t) : Mathf.Lerp(-25f, 0, t));

            yield return null;
        }

        Destroy(fakeCard);
    }
}
