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

    public void ShowForTutorial()
    {
        gameObject.SetActive(true);
        SetVisible(true);
    }

    public void HideForTutorial()
    {
        SetVisible(false);
        gameObject.SetActive(false);
    }

    private void SetVisible(bool visible)
    {
        if (gamepadImage != null) gamepadImage.SetActive(visible);
        if (keyboardImage != null) keyboardImage.SetActive(visible);
        if (controlsImage != null) controlsImage.enabled = visible;
        if (gamepadText != null) gamepadText.gameObject.SetActive(visible);
        if (keyboardText != null) keyboardText.gameObject.SetActive(visible);
    }
}
