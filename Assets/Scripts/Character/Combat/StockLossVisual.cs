using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Screen-space KO cue: the fighter may already be outside the camera when the stock is confirmed.
public sealed class StockLossVisual : MonoBehaviour
{
    private const float Duration = 0.64f;
    private const int RayWidth = 256;
    private const int RayHeight = 64;
    private const int RayCount = 8;
    private const float RaySpacingScale = 0.65f;

    private RectTransform canvasRect;
    private AudioSource audioSource;
    private Sprite raySprite;
    private AudioClip customCue;

    private void Awake()
    {
        GameObject canvasObject = new GameObject("Stock Loss Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10; // Keep the combat HUD readable.
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        raySprite = BuildRaySprite();
        customCue = Resources.Load<AudioClip>("StockLoss");
        if (customCue != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            GameSettings.RegisterSource(audioSource, GameSound.SoundEffects);
        }
    }

    public void Play(Vector2 worldPosition, Vector2 velocity, Color playerColor, bool finalStock)
    {
        Camera camera = Camera.main;
        Vector2 viewport = camera != null ? camera.WorldToViewportPoint(worldPosition) : new Vector2(0.5f, -0.2f);
        Vector2 outward = viewport - new Vector2(0.5f, 0.5f);
        if (outward.sqrMagnitude < 0.0025f)
            outward = velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector2.down;

        Vector2 border = BorderPoint(outward);
        Vector2 direction = Mathf.Abs(outward.x) > Mathf.Abs(outward.y)
            ? new Vector2(Mathf.Sign(outward.x), 0f)
            : new Vector2(0f, Mathf.Sign(outward.y));
        Vector2 canvasSize = canvasRect.rect.size;
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
            canvasSize = new Vector2(1920f, 1080f);
        float screenSpan = canvasSize.magnitude;

        GameObject effect = new GameObject("KO Rays", typeof(RectTransform));
        RectTransform root = effect.GetComponent<RectTransform>();
        root.SetParent(canvasRect, false);
        root.anchorMin = border;
        root.anchorMax = border;
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = Vector2.zero;

        Image[] glows = new Image[RayCount];
        Image[] cores = new Image[RayCount];
        Vector2[] centers = new Vector2[RayCount];
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, baseAngle);
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        for (int i = 0; i < RayCount; i++)
        {
            bool mainRay = i < 2;
            int pair = (i - 2) / 2;
            float side = i % 2 == 0 ? -1f : 1f;
            float offset = mainRay ? (i == 0 ? -0.025f : 0.035f)
                : side * (0.075f + 0.038f * pair);
            float inward = mainRay ? (i == 0 ? 0.26f : 0.21f)
                : 0.18f + 0.025f * pair;
            centers[i] = -direction * (screenSpan * inward)
                + perpendicular * (screenSpan * offset * RaySpacingScale);
            float length = screenSpan * (mainRay ? (i == 0 ? 1.14f : 0.94f)
                : 0.75f - 0.1f * pair);
            float glowWidth = mainRay ? (i == 0 ? 104f : 76f) : 42f - 8f * pair;
            float coreWidth = mainRay ? (i == 0 ? 28f : 20f) : 12f - 2f * pair;

            glows[i] = CreateRay("KO Ray Glow", root, new Vector2(length, glowWidth),
                centers[i], rotation);
            cores[i] = CreateRay("KO Ray Core", root, new Vector2(length * 0.96f, coreWidth),
                centers[i], rotation);
        }

        if (audioSource != null && customCue != null && !PauseMenu.isPaused)
            audioSource.PlayOneShot(customCue, finalStock ? 1f : 0.8f);

        StartCoroutine(Animate(effect, glows, cores, centers, direction, screenSpan,
            playerColor, finalStock));
    }

    private IEnumerator Animate(GameObject effect, Image[] glows, Image[] cores,
        Vector2[] centers, Vector2 direction, float screenSpan, Color color, bool finalStock)
    {
        float elapsed = 0f;
        float size = finalStock ? 1.12f : 1f;
        Color coreColor = Color.Lerp(color, Color.white, 0.88f);
        while (elapsed < Duration)
        {
            if (PauseMenu.isPaused)
            {
                effect.SetActive(false);
                yield return null;
                continue;
            }

            if (!effect.activeSelf)
                effect.SetActive(true);
            elapsed += Time.deltaTime;

            for (int i = 0; i < RayCount; i++)
            {
                bool mainRay = i < 2;
                float delay = mainRay ? i * 0.035f : 0.015f + (i - 2) * 0.022f;
                float rayTime = elapsed - delay;
                float appear = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01(rayTime / (mainRay ? 0.09f : 0.055f)));
                float fade = 1f - Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((rayTime - (mainRay ? 0.17f : 0.11f))
                        / (mainRay ? 0.37f : 0.29f)));
                float alpha = appear * fade;
                Vector3 scale = new Vector3(size * Mathf.Lerp(mainRay ? 0.18f : 0.07f,
                    1f, appear), size, 1f);
                Vector2 position = centers[i] + direction * (screenSpan
                    * (mainRay ? 0.055f : 0.13f)
                    * Mathf.Clamp01(rayTime / (mainRay ? 0.26f : 0.18f)));
                glows[i].rectTransform.localScale = scale;
                cores[i].rectTransform.localScale = scale;
                glows[i].rectTransform.anchoredPosition = position;
                cores[i].rectTransform.anchoredPosition = position;
                glows[i].color = new Color(color.r, color.g, color.b,
                    alpha * (mainRay ? (i == 0 ? 0.58f : 0.43f) : 0.56f));
                cores[i].color = new Color(coreColor.r, coreColor.g, coreColor.b,
                    alpha * (mainRay ? (i == 0 ? 0.96f : 0.8f) : 0.9f));
            }

            yield return null;
        }

        Destroy(effect);
    }

    private static Vector2 BorderPoint(Vector2 outward)
    {
        float xScale = Mathf.Abs(outward.x) > 0.0001f ? 0.39f / Mathf.Abs(outward.x) : float.PositiveInfinity;
        float yScale = Mathf.Abs(outward.y) > 0.0001f ? 0.39f / Mathf.Abs(outward.y) : float.PositiveInfinity;
        return new Vector2(0.5f, 0.5f) + outward * Mathf.Min(xScale, yScale);
    }

    private Image CreateRay(string name, RectTransform parent, Vector2 size,
        Vector2 position, Quaternion rotation)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localRotation = rotation;
        Image image = child.GetComponent<Image>();
        image.sprite = raySprite;
        image.color = Color.clear;
        image.raycastTarget = false;
        return image;
    }

    private static Sprite BuildRaySprite()
    {
        Color32[] pixels = new Color32[RayWidth * RayHeight];
        for (int y = 0; y < RayHeight; y++)
        for (int x = 0; x < RayWidth; x++)
        {
            float along = Mathf.Abs((x + 0.5f) / RayWidth * 2f - 1f);
            float across = Mathf.Abs((y + 0.5f) / RayHeight * 2f - 1f);
            float width = 0.82f * Mathf.Pow(1f - along, 0.55f);
            float edge = Mathf.Clamp01((width - across) / 0.2f);
            float tip = Mathf.Clamp01((1f - along) / 0.08f);
            byte alpha = (byte)Mathf.RoundToInt(255f * edge * tip);
            pixels[y * RayWidth + x] = new Color32(255, 255, 255, alpha);
        }

        Texture2D texture = new Texture2D(RayWidth, RayHeight, TextureFormat.RGBA32, false)
        {
            name = "KO Ray",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, RayWidth, RayHeight),
            new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private void OnDestroy()
    {
        if (raySprite == null)
            return;

        Texture2D texture = raySprite.texture;
        Destroy(raySprite);
        Destroy(texture);
    }
}
