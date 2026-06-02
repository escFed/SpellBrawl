using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class UICard : MonoBehaviour, IPointerClickHandler
{
    public GameObject cardPrefab;
    public DeckBuilderUI deckBuilder;
    public TextMeshProUGUI cardCopiesText;

<<<<<<< Updated upstream:Assets/Scripts/UI/CardsSelect/UICard.cs
=======

    [Header("Animation")]
    public RectTransform deckTarget;
    public Canvas canvas;
    public Image cardImage;


    private int currentCopies = 0;

>>>>>>> Stashed changes:Assets/Scripts/UI/UICard.cs
    private void Start() => UpdateVisuals();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (deckBuilder.AddCardToDeck(cardPrefab))
            {
<<<<<<< Updated upstream:Assets/Scripts/UI/CardsSelect/UICard.cs
=======
                AnimateCardToDeck();

                currentCopies++;
>>>>>>> Stashed changes:Assets/Scripts/UI/UICard.cs
                UpdateVisuals();
            }
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
<<<<<<< Updated upstream:Assets/Scripts/UI/CardsSelect/UICard.cs
            deckBuilder.RemoveCard(cardPrefab);
            UpdateVisuals();
=======
            if (currentCopies > 0)
            {
                AnimateCardFromDeck();
                deckBuilder.RemoveCard(cardPrefab);
                currentCopies--;
                UpdateVisuals();
            }
>>>>>>> Stashed changes:Assets/Scripts/UI/UICard.cs
        }
    }

    public void UpdateVisuals()
    {
        int count = deckBuilder.GetCardCount(cardPrefab);
        if (cardCopiesText != null)
            cardCopiesText.text = count.ToString() + "/5";
    }
    void AnimateCardToDeck()
    {
        GameObject fakeCard = new GameObject("FakeCard");

        fakeCard.transform.SetParent(canvas.transform);

        Image image = fakeCard.AddComponent<Image>();

        image.sprite = cardImage.sprite;
        image.preserveAspect = true;

        RectTransform fakeRect =
            fakeCard.GetComponent<RectTransform>();

        RectTransform originalRect =
            GetComponent<RectTransform>();

        fakeRect.sizeDelta = originalRect.sizeDelta;

        fakeRect.position = originalRect.position;

        StartCoroutine(MoveFakeCard(fakeRect));
    }

    IEnumerator MoveFakeCard(RectTransform card)
    {
        Vector3 start = card.position;
        Vector3 end = deckTarget.position;

        Vector3 middle = (start + end) / 2f;

        middle.y += 120f;

        float duration = 0.35f;
        float elapsed = 0f;

        Vector3 startScale = card.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            t = 1f - Mathf.Pow(1f - t, 3);

            Vector3 pos =
                Mathf.Pow(1 - t, 2) * start +
                2 * (1 - t) * t * middle +
                Mathf.Pow(t, 2) * end;

            card.position = pos;

            card.localScale =
                Vector3.Lerp(startScale, Vector3.zero, t);

            card.rotation =
                Quaternion.Euler(0, 0, Mathf.Lerp(0, 25f, t));

            yield return null;
        }

        Destroy(card.gameObject);
    }

    void AnimateCardFromDeck()
    {
        GameObject fakeCard = new GameObject("FakeCard");

        fakeCard.transform.SetParent(canvas.transform);

        Image image = fakeCard.AddComponent<Image>();

        image.sprite = cardImage.sprite;
        image.preserveAspect = true;

        RectTransform fakeRect =
            fakeCard.GetComponent<RectTransform>();

        RectTransform originalRect =
            GetComponent<RectTransform>();

        fakeRect.sizeDelta = originalRect.sizeDelta;

        // EMPIEZA EN EL DECK
        fakeRect.position = deckTarget.position;

        StartCoroutine(MoveFakeCardFromDeck(fakeRect, originalRect.position));
    }

    IEnumerator MoveFakeCardFromDeck(RectTransform card, Vector3 target)
    {
        Vector3 start = deckTarget.position;
        Vector3 end = target;

        Vector3 middle = (start + end) / 2f;

        middle.y += 120f;

        float duration = 0.35f;
        float elapsed = 0f;

        // Empieza chica
        card.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            t = 1f - Mathf.Pow(1f - t, 3);

            Vector3 pos =
                Mathf.Pow(1 - t, 2) * start +
                2 * (1 - t) * t * middle +
                Mathf.Pow(t, 2) * end;

            card.position = pos;

            // Crece mientras viaja
            card.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, t);

            card.rotation =
                Quaternion.Euler(0, 0, Mathf.Lerp(-25f, 0, t));

            yield return null;
        }

        card.position = end;

        Destroy(card.gameObject);

        // Bounce de la carta original
        StartCoroutine(BounceCard());
    }


    IEnumerator BounceCard()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector3 original = Vector3.one;
        Vector3 big = Vector3.one * 1.08f;

        float duration = 0.08f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            rect.localScale =
                Vector3.Lerp(original, big, elapsed / duration);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            rect.localScale =
                Vector3.Lerp(big, original, elapsed / duration);

            yield return null;
        }

        rect.localScale = original;
    }


}
