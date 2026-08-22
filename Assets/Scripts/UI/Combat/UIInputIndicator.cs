using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Image))]
public class UIInputIndicator : MonoBehaviour
{
    [Header("BottonIcon")]
    public Sprite keyboardSprite;
    public Sprite gamepadSprite;

    private Image indicatorImage;
    private PlayerInput p1Input;
    private string lastControlScheme = "";

    private void Awake()
    {
        indicatorImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (p1Input == null)
        {
            if (RespawnManager.Instance != null && RespawnManager.Instance.p1Instance != null)
            {
                p1Input = RespawnManager.Instance.p1Instance.GetComponent<PlayerInput>();
            }
            return;
        }

        string currentScheme = p1Input.currentControlScheme;

        if (currentScheme != lastControlScheme)
        {
            UpdateIndicator(currentScheme);
            lastControlScheme = currentScheme;
        }
    }

    private void UpdateIndicator(string schemeName)
    {
        if (schemeName == "Gamepad")
        {
            if (gamepadSprite != null) indicatorImage.sprite = gamepadSprite;
        }
        else
        {
            if (keyboardSprite != null) indicatorImage.sprite = keyboardSprite;
        }
    }
}
