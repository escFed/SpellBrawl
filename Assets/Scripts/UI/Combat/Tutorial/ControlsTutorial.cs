using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsTutorial : MonoBehaviour
{
    [SerializeField] private GameObject gamepadImage;
    [SerializeField] private GameObject keyboardImage;
    [SerializeField] private Image controlsImage;
    [SerializeField] private TextMeshProUGUI gamepadText;
    [SerializeField] private TextMeshProUGUI keyboardText;
    private Vector2 gamepadPosition;
    private Vector2 keyboardPosition;
    private bool positionsCached;
    private bool useGamepad;

    public void ShowForTutorial(bool isGamepad)
    {
        CachePositions();
        useGamepad = isGamepad;
        gameObject.SetActive(true);
        SetVisible(true);
    }

    public void SetControlType(bool isGamepad)
    {
        useGamepad = isGamepad;
        if (gameObject.activeInHierarchy)
            SetVisible(true);
    }

    public void HideForTutorial()
    {
        SetVisible(false);
        gameObject.SetActive(false);
    }

    private void SetVisible(bool visible)
    {
        if (visible)
        {
            Center(gamepadImage);
            Center(keyboardImage);
        }
        else
        {
            RestorePositions();
        }

        if (gamepadImage != null) gamepadImage.SetActive(visible && useGamepad);
        if (keyboardImage != null) keyboardImage.SetActive(visible && !useGamepad);
        if (controlsImage != null) controlsImage.enabled = visible;
        if (gamepadText != null) gamepadText.gameObject.SetActive(visible && useGamepad);
        if (keyboardText != null) keyboardText.gameObject.SetActive(visible && !useGamepad);
    }

    private void CachePositions()
    {
        if (positionsCached)
            return;

        if (gamepadImage != null && gamepadImage.transform is RectTransform gamepadRect)
            gamepadPosition = gamepadRect.anchoredPosition;
        if (keyboardImage != null && keyboardImage.transform is RectTransform keyboardRect)
            keyboardPosition = keyboardRect.anchoredPosition;
        positionsCached = true;
    }

    private static void Center(GameObject image)
    {
        if (image != null && image.transform is RectTransform rect)
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
    }

    private void RestorePositions()
    {
        if (!positionsCached)
            return;

        if (gamepadImage != null && gamepadImage.transform is RectTransform gamepadRect)
            gamepadRect.anchoredPosition = gamepadPosition;
        if (keyboardImage != null && keyboardImage.transform is RectTransform keyboardRect)
            keyboardRect.anchoredPosition = keyboardPosition;
    }
}
