using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MenuInputIndicator : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite keyboardSprite;
    public Sprite gamepadSprite;

    private Image indicatorImage;

    private void Awake()
    {
        indicatorImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        InputDetector.OnGamepadActive += UpdateIndicator;
    }

    private void OnDisable()
    {
        InputDetector.OnGamepadActive -= UpdateIndicator;
    }

    private void UpdateIndicator(bool isGamepad)
    {
        if (isGamepad)
        {
            if (gamepadSprite != null) indicatorImage.sprite = gamepadSprite;
        }
        else
        {
            if (keyboardSprite != null) indicatorImage.sprite = keyboardSprite;
        }
    }
}
