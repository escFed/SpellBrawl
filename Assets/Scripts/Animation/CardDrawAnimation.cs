using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMenuIntro : MonoBehaviour
{
    [Header("Deck")]
    public RectTransform deckRect;

    public Vector2 deckStartPos;
    public Vector2 deckCenterPos;

    [Header("Cards")]
    public List<RectTransform> cards;

    public List<Vector2> finalPositions;

    [Header("Timing")]
    public float deckMoveDuration = 0.45f;
    public float cardMoveDuration = 0.35f;
    public float delayBetweenCards = 0.07f;

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        DisableCards();

        deckRect.anchoredPosition = deckStartPos;

        yield return StartCoroutine(MoveDeck());

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(true);

            StartCoroutine(MoveCard(cards[i], finalPositions[i]));

            yield return new WaitForSeconds(delayBetweenCards);
        }
    }

    void DisableCards()
    {
        foreach (var card in cards)
        {
            card.gameObject.SetActive(false);
        }
    }

    IEnumerator MoveDeck()
    {
        float elapsed = 0;

        while (elapsed < deckMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / deckMoveDuration;

            t = 1 - Mathf.Pow(1 - t, 3);

            deckRect.anchoredPosition =
                Vector2.Lerp(deckStartPos, deckCenterPos, t);

            yield return null;
        }

        deckRect.anchoredPosition = deckCenterPos;
    }

    IEnumerator MoveCard(RectTransform card, Vector2 target)
    {
        card.anchoredPosition = deckCenterPos;

        float elapsed = 0;

        Vector2 start = deckCenterPos;

        Vector2 middle = (start + target) / 2f;
        middle.y += Random.Range(60f, 140f);

        float startRot = Random.Range(-25f, 25f);

        while (elapsed < cardMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / cardMoveDuration;

            float curveT = 1 - Mathf.Pow(1 - t, 3);

            Vector2 pos =
                Mathf.Pow(1 - curveT, 2) * start +
                2 * (1 - curveT) * curveT * middle +
                Mathf.Pow(curveT, 2) * target;

            card.anchoredPosition = pos;

            float rot = Mathf.Lerp(startRot, 0, curveT);

            card.localRotation = Quaternion.Euler(0, 0, rot);

            yield return null;
        }

        card.anchoredPosition = target;
        card.localRotation = Quaternion.identity;

        StartCoroutine(Bounce(card));
    }

    IEnumerator Bounce(RectTransform card)
    {
        Vector3 original = Vector3.one;
        Vector3 big = Vector3.one * 1.08f;

        float duration = 0.08f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            card.localScale =
                Vector3.Lerp(original, big, elapsed / duration);

            yield return null;
        }

        elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            card.localScale =
                Vector3.Lerp(big, original, elapsed / duration);

            yield return null;
        }

        card.localScale = original;
    }
}