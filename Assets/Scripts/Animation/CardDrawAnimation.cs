using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardDrawAnimation : MonoBehaviour
{
    [Header("Referencias")]
    public Transform deckTransform;

    [Header("Casillas de Cartas (UI)")]
    public Transform[] p1CardSlots;
    public Transform[] p2CardSlots;

    public Image cardPrefab;

    [Header("Deck Movement")]
    public Vector3 deckStartPosition;
    public Vector3 deckCenterPosition;

    [Header("Cards")]
    public int cardsPerPlayer = 4;

    [Header("Timing")]
    public float deckMoveDuration = 0.5f;
    public float delayBeforeCards = 0.2f;
    public float delayBetweenCards = 0.08f;
    public float cardTravelDuration = 0.5f;




    public void PlayDrawAnimation()
    {
        StartCoroutine(StartRoundAnimation());
    }

    IEnumerator StartRoundAnimation()
    {
        deckTransform.gameObject.SetActive(true);
        deckTransform.position = deckStartPosition;

        yield return StartCoroutine(MoveDeckToCenter());
        yield return new WaitForSeconds(delayBeforeCards);

        for (int i = 0; i < cardsPerPlayer; i++)
        {
            if (p1CardSlots != null && i < p1CardSlots.Length && p1CardSlots[i] != null)
            {
               
                SpawnCard(p1CardSlots[i]);
                yield return new WaitForSeconds(delayBetweenCards);
            }

            if (p2CardSlots != null && i < p2CardSlots.Length && p2CardSlots[i] != null)
            {
                
                SpawnCard(p2CardSlots[i]);
                yield return new WaitForSeconds(delayBetweenCards);
            }
        }

        yield return new WaitForSeconds(0.5f);
        deckTransform.gameObject.SetActive(false);
        // 👉 Aquí llamás a la animación de las cartas iniciales
        UIManager.Instance.AnimateInitialCards(0); // jugador 1
        UIManager.Instance.AnimateInitialCards(1); // jugador 2

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
        Vector3 squishScale = new Vector3(originalScale.x * 1.1f, originalScale.y * 0.9f, originalScale.z);

        float duration = 0.08f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            deckTransform.localScale = Vector3.Lerp(originalScale, squishScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            deckTransform.localScale = Vector3.Lerp(squishScale, originalScale, t);
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

     
            elapsed += Time.deltaTime;
            float t = elapsed / cardTravelDuration;
            float curveT = 1f - Mathf.Pow(1f - t, 3);

            LeanTween.move(card.gameObject, middle, cardTravelDuration * 0.5f)
          .setEase(LeanTweenType.easeOutQuad)
          .setOnComplete(() =>
          {
              LeanTween.move(card.gameObject, target.position, cardTravelDuration * 0.5f)
                       .setEase(LeanTweenType.easeInQuad);
          });

        yield return new WaitForSeconds(cardTravelDuration);
        

        yield return StartCoroutine(CardAbsorb(card));
    }

    IEnumerator CardAbsorb(Transform card)
    {
        Vector3 startScale = card.localScale;
        Vector3 endScale = Vector3.zero;
        float duration = 0.12f;
        float elapsed = 0f;

      
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
           

        LeanTween.scale(card.gameObject, endScale, duration)
                 .setEase(LeanTweenType.easeInBack);

        yield return null;
        

        Destroy(card.gameObject);
    }
}