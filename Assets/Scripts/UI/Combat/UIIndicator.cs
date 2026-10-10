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

    [Header("Offscreen")]
    [SerializeField, Range(0.02f, 0.25f)] private float viewportMargin = 0.08f;

    private Transform myParent;
    private Vector3 originalScale;
    private RectTransform indicatorRect;
    private Vector2 originalAnchoredPosition;
    private Quaternion originalArrowRotation;
    private Canvas indicatorCanvas;
    private CharacterCoordinator character;
    private Camera gameCamera;
    private bool wasOffscreen;

    private void Start()
    {
        myParent = transform.parent;
        originalScale = transform.localScale;
        indicatorRect = GetComponent<RectTransform>();
        originalAnchoredPosition = indicatorRect.anchoredPosition;
        originalArrowRotation = arrowImage != null ? arrowImage.rectTransform.localRotation : Quaternion.identity;
        indicatorCanvas = GetComponent<Canvas>();
        gameCamera = Camera.main;

        character = GetComponentInParent<CharacterCoordinator>();
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
        UpdateOffscreenPosition();
    }

    private void UpdateOffscreenPosition()
    {
        if (character == null || indicatorCanvas == null)
            return;

        bool shouldShow = character.Health != null && character.Health.isActiveAndEnabled &&
            character.Health.ShouldCameraTrack;
        indicatorCanvas.enabled = shouldShow;
        if (!shouldShow)
            return;

        if (gameCamera == null)
            gameCamera = Camera.main;
        if (gameCamera == null)
            return;

        Vector3 markerPosition = myParent.TransformPoint(new Vector3(
            originalAnchoredPosition.x, originalAnchoredPosition.y, 0f));
        Vector3 viewport = gameCamera.WorldToViewportPoint(markerPosition);
        float margin = viewportMargin;
        bool inView = viewport.z > 0f &&
            viewport.x >= margin && viewport.x <= 1f - margin &&
            viewport.y >= margin && viewport.y <= 1f - margin;

        if (inView)
        {
            if (wasOffscreen)
            {
                indicatorRect.anchoredPosition = originalAnchoredPosition;
                if (arrowImage != null)
                    arrowImage.rectTransform.localRotation = originalArrowRotation;
                wasOffscreen = false;
            }
            return;
        }

        if (viewport.z <= 0f)
        {
            viewport.x = 1f - viewport.x;
            viewport.y = 1f - viewport.y;
        }

        Vector2 edge = new Vector2(
            Mathf.Clamp(viewport.x, margin, 1f - margin),
            Mathf.Clamp(viewport.y, margin, 1f - margin));
        float depth = Vector3.Dot(markerPosition - gameCamera.transform.position, gameCamera.transform.forward);
        indicatorRect.position = gameCamera.ViewportToWorldPoint(new Vector3(edge.x, edge.y, Mathf.Max(0.01f, depth)));

        if (arrowImage != null)
        {
            Vector2 direction = new Vector2(viewport.x - edge.x, viewport.y - edge.y);
            float angle = Vector2.SignedAngle(Vector2.down, direction);
            arrowImage.rectTransform.localRotation = originalArrowRotation * Quaternion.Euler(0f, 0f, angle);
        }

        wasOffscreen = true;
    }
}
