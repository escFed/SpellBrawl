using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardDrawAnimation : MonoBehaviour
{
    [Header("Referencias")]
    public Transform deckTransform;

    // Targets de los 4 jugadores
    public Transform[] playerTargets;

    public Image cardPrefab;

    [Header("Deck Movement")]
    public Vector3 deckStartPosition;
    public Vector3 deckCenterPosition;

    [Header("Cards")]
    public int cardsPerPlayer = 3;

    [Header("Timing")]
    public float deckMoveDuration = 0.5f;
    public float delayBeforeCards = 0.2f;
    public float delayBetweenCards = 0.08f;
    public float cardTravelDuration = 0.5f;

    private void Start()
    {
        StartCoroutine(StartRoundAnimation());
    }

    IEnumerator StartRoundAnimation()
    {
        deckTransform.gameObject.SetActive(true);

        deckTransform.position = deckStartPosition;

        yield return StartCoroutine(MoveDeckToCenter());

        yield return new WaitForSeconds(delayBeforeCards);

        int totalCards = cardsPerPlayer * playerTargets.Length;

        for (int i = 0; i < totalCards; i++)
        {
            Transform target = playerTargets[i % playerTargets.Length];

            SpawnCard(target);

            yield return new WaitForSeconds(delayBetweenCards);
        }

        yield return new WaitForSeconds(0.5f);

        deckTransform.gameObject.SetActive(false);
    }

    IEnumerator MoveDeckToCenter()
    {
        float elapsed = 0f;

        Vector3 startPos = deckStartPosition;
        Vector3 endPos = deckCenterPosition;

        while (elapsed < deckMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / deckMoveDuration;

            t = 1f - Mathf.Pow(1f - t, 3);

            deckTransform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        deckTransform.position = endPos;

        yield return StartCoroutine(DeckBounce());
    }

    IEnumerator DeckBounce()
    {
        Vector3 originalScale = deckTransform.localScale;

        Vector3 squishScale = new Vector3(
            originalScale.x * 1.1f,
            originalScale.y * 0.9f,
            originalScale.z
        );

        float duration = 0.08f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            deckTransform.localScale =
                Vector3.Lerp(originalScale, squishScale, t);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            deckTransform.localScale =
                Vector3.Lerp(squishScale, originalScale, t);

            yield return null;
        }

        deckTransform.localScale = originalScale;
    }

    void SpawnCard(Transform target)
    {
        Image card = Instantiate(cardPrefab, deckTransform.position, Quaternion.identity, deckTransform.parent);

        StartCoroutine(MoveCard(card.transform, target));
    }

    IEnumerator MoveCard(Transform card, Transform target)
    {
        Vector3 start = deckTransform.position;

        Vector3 end = target.position;

        Vector3 middle = (start + end) / 2f;

        middle.x += Random.Range(-1f, 1f);
        middle.y += Random.Range(1.5f, 3f);

        float elapsed = 0f;

        float startRotation = Random.Range(-40f, 40f);

        while (elapsed < cardTravelDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / cardTravelDuration;

            float curveT = 1f - Mathf.Pow(1f - t, 3);

            Vector3 position =
                Mathf.Pow(1 - curveT, 2) * start +
                2 * (1 - curveT) * curveT * middle +
                Mathf.Pow(curveT, 2) * end;

            card.position = position;

            float rotation = Mathf.Lerp(startRotation, 0f, curveT);

            card.rotation = Quaternion.Euler(0, 0, rotation);

            yield return null;
        }

        yield return StartCoroutine(CardAbsorb(card));
    }

    IEnumerator CardAbsorb(Transform card)
    {
        Vector3 startScale = card.localScale;

        Vector3 endScale = Vector3.zero;

        float duration = 0.12f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            card.localScale =
                Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        Destroy(card.gameObject);
    }
}