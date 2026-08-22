using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CardAnimationsUI : MonoBehaviour
{
    [SerializeField] private GameObject[] cards = new GameObject[4];

    private Vector2[] originalPositions;

    void Awake()
    {
        // Guardar las posiciones iniciales de cada carta
        originalPositions = new Vector2[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
                originalPositions[i] = cards[i].GetComponent<RectTransform>().anchoredPosition;
        }
    }

    void OnEnable()
    {
        UIEvents.OnHandChanged += ResetCardPositions;
    }

    void OnDisable()
    {
        UIEvents.OnHandChanged -= ResetCardPositions;
    }

    public void OnCardInteraction(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cards.Length) return;

        RectTransform cardTransform = cards[cardIndex].GetComponent<RectTransform>();
        Vector2 originalPos = originalPositions[cardIndex];

        LeanTween.moveY(cardTransform, originalPos.y + 200f, 0.15f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
             LeanTween.moveY(cardTransform, originalPos.y, 0.15f).setEase(LeanTweenType.easeInQuad);
        });
    }

    private void ResetCardPositions(int playerIndex, HandSlotView[] hand)
    {
        // Cada vez que se roba/redibuja la mano, todas las cartas vuelven a su posición inicial
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;
            RectTransform cardTransform = cards[i].GetComponent<RectTransform>();
            cardTransform.anchoredPosition = originalPositions[i];
        }
    }
}
