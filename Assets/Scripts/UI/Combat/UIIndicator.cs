using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIIndicator : MonoBehaviour
{
    public TextMeshProUGUI indicatorText;
    public Image arrowImage;

    [Header("P1")]
    public string p1Name = "P1";
    public Color p1Color = new Color(1f, 0.2f, 0.2f);

    [Header("IA")]
    public string iaName = "IA";
    public Color iaColor = new Color(0.2f, 0.4f, 1f);

    private Transform myParent;
    private Vector3 originalScale;

    private void Start()
    {
        myParent = transform.parent;
        originalScale = transform.localScale;


        PlayerController character = GetComponentInParent<PlayerController>();

        if (character != null)
        {
            if (character.PlayerIndex == 0)
            {
                if (indicatorText != null) indicatorText.text = p1Name;
                if (indicatorText != null) indicatorText.color = p1Color;
                if (arrowImage != null) arrowImage.color = p1Color;
            }
            else 
            {
                if (indicatorText != null) indicatorText.text = iaName;
                if (indicatorText != null) indicatorText.color = iaColor;
                if (arrowImage != null) arrowImage.color = iaColor;
            }
        }
    }

    private void LateUpdate()
    {
        if (myParent == null) return;

        transform.rotation = Quaternion.identity;

        float fixX = myParent.localScale.x < 0 ? -1f : 1f;

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * fixX,
            originalScale.y,
            originalScale.z
        );
    }
}
