using UnityEngine;
using TMPro;

public class UIIndicator : MonoBehaviour
{
    public TextMeshProUGUI indicatorText;
    public UnityEngine.UI.Image arrowImage;

    [Header("P1")]
    public string p1Name = "P1";
    [HideInInspector]
    public Color p1Color = new Color(1f, 0.2f, 0.2f);

    [Header("P2")]
    public string p2Name = "P2";
    [HideInInspector]
    public Color p2Color = new Color(0.2f, 0.4f, 1f);

    [Header("CPU")]
    public string iaName = "CPU";
    [HideInInspector]
    public Color iaColor = new Color(0.2f, 0.4f, 1f);

    [Header("Offscreen")]
    [SerializeField, Range(0.02f, 0.25f)] private float viewportMargin = 0.08f;
    [SerializeField, Min(0.01f)] private float portraitTransitionDuration = 0.16f;
    [SerializeField, Min(0f)] private float koWarningDistance = 2.5f;

    private static readonly Color PortraitBackground = new Color(0.055f, 0.065f, 0.1f, 0.94f);
    private static readonly Color KoWarningColor = new Color32(255, 194, 54, 255);
    private static Sprite circleSprite;

    private Transform myParent;
    private Vector3 originalScale;
    private RectTransform indicatorRect;
    private Vector2 originalAnchoredPosition;
    private Quaternion originalArrowRotation;
    private Vector3 originalArrowScale;
    private Canvas indicatorCanvas;
    private CharacterCoordinator character;
    private Camera gameCamera;
    private CanvasGroup portraitGroup;
    private RectTransform portraitRect;
    private UnityEngine.UI.Image portraitFrame;
    private Collider2D koBounds;
    private Collider2D bottomKoZone;
    private Color playerColor = Color.white;
    private bool wasOffscreen;

    private void Start()
    {
        myParent = transform.parent;
        originalScale = transform.localScale;
        indicatorRect = GetComponent<RectTransform>();
        originalAnchoredPosition = indicatorRect.anchoredPosition;
        originalArrowRotation = arrowImage != null ? arrowImage.rectTransform.localRotation : Quaternion.identity;
        originalArrowScale = arrowImage != null ? arrowImage.rectTransform.localScale : Vector3.one;
        indicatorCanvas = GetComponent<Canvas>();
        gameCamera = Camera.main;

        if (arrowImage != null)
        {
            UnityEngine.UI.Outline outline = arrowImage.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null)
                outline = arrowImage.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = PortraitBackground;
            outline.effectDistance = new Vector2(2f, -2f);
            arrowImage.raycastTarget = false;
        }

        character = GetComponentInParent<CharacterCoordinator>();
        if (character != null)
        {
            Configure(character.Slot, character.Mode);
            CreatePortrait(character.Stats != null ? character.Stats.characterIcon : null);
        }

        InstaKillZone outerZone = FindAnyObjectByType<InstaKillZone>();
        RespawnZone lowerZone = FindAnyObjectByType<RespawnZone>();
        koBounds = outerZone != null ? outerZone.GetComponent<Collider2D>() : null;
        bottomKoZone = lowerZone != null ? lowerZone.GetComponent<Collider2D>() : null;
    }

    public void Configure(PlayerSlot slot, PlayerMode mode)
    {
        // Slot colors match the combat HUD even when the opponent is controlled by AI.
        playerColor = PlayerColors.Get((int)slot);
        if (mode == PlayerMode.AI)
        {
            ApplyPresentation(iaName, playerColor);
            return;
        }

        if (slot == PlayerSlot.PlayerOne)
            ApplyPresentation(p1Name, playerColor);
        else
            ApplyPresentation(p2Name, playerColor);
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

        if (portraitFrame != null)
            portraitFrame.color = color;
    }

    private void CreatePortrait(Sprite icon)
    {
        if (icon == null || indicatorRect == null)
            return;

        GameObject portraitObject = new GameObject("OffscreenPortrait", typeof(RectTransform), typeof(CanvasGroup));
        portraitRect = portraitObject.GetComponent<RectTransform>();
        portraitRect.SetParent(indicatorRect, false);
        portraitRect.SetAsFirstSibling();
        portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        portraitRect.anchoredPosition = new Vector2(-60f, 0f);
        portraitRect.sizeDelta = new Vector2(72f, 72f);
        portraitGroup = portraitObject.GetComponent<CanvasGroup>();
        portraitGroup.alpha = 0f;
        portraitGroup.interactable = false;
        portraitGroup.blocksRaycasts = false;

        Sprite circle = GetCircleSprite();
        portraitFrame = CreatePortraitImage("Frame", portraitRect, 72f, playerColor);
        portraitFrame.sprite = circle;
        UnityEngine.UI.Image background = CreatePortraitImage("Background", portraitRect, 64f, PortraitBackground);
        background.sprite = circle;
        UnityEngine.UI.Mask mask = background.gameObject.AddComponent<UnityEngine.UI.Mask>();
        mask.showMaskGraphic = true;
        UnityEngine.UI.Image portrait = CreatePortraitImage("Character", background.rectTransform, 64f, Color.white);
        portrait.sprite = icon;
        portrait.preserveAspect = true;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 72;
        Color32[] pixels = new Color32[size * size];
        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                byte alpha = (byte)(255f * Mathf.Clamp01(center - Mathf.Sqrt(dx * dx + dy * dy)));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
    }

    private static UnityEngine.UI.Image CreatePortraitImage(string name, RectTransform parent, float size, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(size, size);
        UnityEngine.UI.Image image = imageObject.GetComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
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
        {
            if (portraitGroup != null)
                portraitGroup.alpha = 0f;
            return;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;
        if (gameCamera == null)
            return;

        Vector3 playerPosition = character.transform.position;
        Vector3 viewport = gameCamera.WorldToViewportPoint(playerPosition);
        bool inView = IsCharacterInView(viewport);

        if (inView)
        {
            if (wasOffscreen)
            {
                indicatorRect.anchoredPosition = originalAnchoredPosition;
                if (arrowImage != null)
                    arrowImage.rectTransform.localRotation = originalArrowRotation;
                wasOffscreen = false;
            }
            UpdatePortrait(false, 0f);
            return;
        }

        if (viewport.z <= 0f)
        {
            viewport.x = 1f - viewport.x;
            viewport.y = 1f - viewport.y;
        }

        Vector2 margin = GetViewportMargins();
        Vector2 edge = new Vector2(
            Mathf.Clamp(viewport.x, margin.x, 1f - margin.x),
            Mathf.Clamp(viewport.y, margin.y, 1f - margin.y));
        float depth = Vector3.Dot(playerPosition - gameCamera.transform.position, gameCamera.transform.forward);
        indicatorRect.position = gameCamera.ViewportToWorldPoint(new Vector3(edge.x, edge.y, Mathf.Max(0.01f, depth)));

        if (arrowImage != null)
        {
            Vector2 direction = new Vector2(viewport.x - edge.x, viewport.y - edge.y);
            float angle = Vector2.SignedAngle(Vector2.down, direction);
            arrowImage.rectTransform.localRotation = originalArrowRotation * Quaternion.Euler(0f, 0f, angle);
        }

        UpdatePortrait(true, GetKoDanger(playerPosition));
        wasOffscreen = true;
    }

    private bool IsCharacterInView(Vector3 playerViewport)
    {
        if (playerViewport.z <= 0f)
            return false;

        SpriteRenderer sprite = character.Sprite;
        if (sprite == null || !sprite.enabled || sprite.sprite == null)
            return playerViewport.x >= 0f && playerViewport.x <= 1f &&
                playerViewport.y >= 0f && playerViewport.y <= 1f;

        Bounds bounds = sprite.bounds;
        Vector3 bottomLeft = gameCamera.WorldToViewportPoint(bounds.min);
        Vector3 topRight = gameCamera.WorldToViewportPoint(bounds.max);
        return Mathf.Min(bottomLeft.x, topRight.x) <= 1f && Mathf.Max(bottomLeft.x, topRight.x) >= 0f &&
            Mathf.Min(bottomLeft.y, topRight.y) <= 1f && Mathf.Max(bottomLeft.y, topRight.y) >= 0f;
    }

    private Vector2 GetViewportMargins()
    {
        Vector2 margin = Vector2.one * viewportMargin;
        if (gameCamera.orthographic)
        {
            Vector3 scale = indicatorRect.lossyScale;
            float viewHeight = 2f * gameCamera.orthographicSize;
            float viewWidth = viewHeight * gameCamera.aspect;
            margin.x = Mathf.Max(margin.x, indicatorRect.rect.width * Mathf.Abs(scale.x) / (2f * viewWidth) + 0.015f);
            margin.y = Mathf.Max(margin.y, indicatorRect.rect.height * Mathf.Abs(scale.y) / (2f * viewHeight) + 0.015f);
        }

        return new Vector2(Mathf.Min(margin.x, 0.45f), Mathf.Min(margin.y, 0.45f));
    }

    private float GetKoDanger(Vector3 position)
    {
        if (koWarningDistance <= 0f || koBounds == null || !koBounds.isActiveAndEnabled)
            return 0f;

        Bounds bounds = koBounds.bounds;
        float bottom = bottomKoZone != null && bottomKoZone.isActiveAndEnabled
            ? bottomKoZone.bounds.max.y
            : bounds.min.y;
        float horizontal = Mathf.Min(bounds.max.x - position.x, position.x - bounds.min.x);
        float vertical = Mathf.Min(bounds.max.y - position.y, position.y - bottom);
        float distance = Mathf.Min(horizontal, vertical);
        return 1f - Mathf.Clamp01(distance / koWarningDistance);
    }

    private void UpdatePortrait(bool offscreen, float danger)
    {
        if (portraitGroup != null)
        {
            float target = offscreen ? 1f : 0f;
            portraitGroup.alpha = offscreen
                ? Mathf.MoveTowards(portraitGroup.alpha, target,
                    Time.unscaledDeltaTime / Mathf.Max(0.01f, portraitTransitionDuration))
                : 0f;
            portraitRect.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, portraitGroup.alpha);
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f);
        Color accent = Color.Lerp(playerColor, KoWarningColor, danger * (0.45f + 0.55f * pulse));
        if (portraitFrame != null)
            portraitFrame.color = accent;
        if (arrowImage != null)
        {
            arrowImage.color = accent;
            float visibility = portraitGroup != null ? portraitGroup.alpha : (offscreen ? 1f : 0f);
            arrowImage.rectTransform.localScale = Vector3.Lerp(originalArrowScale, originalArrowScale * 1.35f, visibility);
        }
    }
}
