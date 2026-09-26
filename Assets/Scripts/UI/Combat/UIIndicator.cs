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

    [Header("P2")]
    public string p2Name = "P2";
    public Color p2Color = new Color(0.2f, 0.4f, 1f);

    [Header("IA")]
    public string iaName = "IA";
    public Color iaColor = new Color(0.2f, 0.4f, 1f);

    private Transform myParent;
    private Vector3 originalScale;

    private void Start()
    {
        myParent = transform.parent;
        originalScale = transform.localScale;

        CharacterCoordinator character = GetComponentInParent<CharacterCoordinator>();
        if (character != null)
            Configure(character.Slot, character.Mode);
    }

    public void Configure(PlayerSlot slot, PlayerMode mode)
    {
        if (mode == PlayerMode.AI)
        {
            ApplyPresentation(iaName, iaColor);
            return;
        }

        if (slot == PlayerSlot.PlayerOne)
            ApplyPresentation(p1Name, p1Color);
        else
            ApplyPresentation(p2Name, p2Color);
    }

    private void ApplyPresentation(string label, Color color)
    {
        if (indicatorText != null)
        {
            indicatorText.text = label;
            indicatorText.color = color;
        }

        if (arrowImage != null)
            arrowImage.color = color;
    }

    private void LateUpdate()
    {
        if (myParent == null) return;

        transform.rotation = Quaternion.identity;

        float fixX = myParent.localScale.x < 0 ? -1f : 1f;

        transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * fixX, originalScale.y, originalScale.z);
    }
}
