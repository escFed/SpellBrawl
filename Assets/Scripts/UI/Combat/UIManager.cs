using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Controls Image")]
    public ControlsTutorial controlsTutorial;

    [Header("Character Icons")]
    public Image p1_icon;
    public Image p2_icon;

    [Header("Control Labels")]
    [SerializeField] private TextMeshProUGUI p1ControlLabel;
    [SerializeField] private TextMeshProUGUI p2ControlLabel;

    [Header("Damage")]
    public TextMeshProUGUI p1_damagePercent;
    public TextMeshProUGUI p2_damagePercent;
    [SerializeField] private Sprite damageFlameSprite;
    private Image p1DamageFlame;
    private Image p2DamageFlame;
    private const int DamageFlameThreshold = 100;

    [Header("Cards P1")]
    public Image[] p1_cards = new Image[4];

    [Header("Cards P2")]
    public Image[] p2_cards = new Image[4];

    [Header("Last Used Card")]
    public TextMeshProUGUI p1LastCardText;
    public TextMeshProUGUI p2LastCardText;

    [Header("Ability Icons")]
    public Image p1DashIcon;
    public Image p1ShieldIcon;
    public Image p2DashIcon;
    public Image p2ShieldIcon;

    [Header("Life")]
    public GameObject[] p1_life = new GameObject[3];
    public GameObject[] p2_life = new GameObject[3];

    [Header("Energy")]
    public Slider p1_energySlider;
    public Slider p2_energySlider;

    [Header("Energy UI")]
    public TextMeshProUGUI p1EnergyText;
    public TextMeshProUGUI p2EnergyText;

    [Header("Deck UI")]
    public TMPro.TextMeshProUGUI p1_deckCountText;
    public TMPro.TextMeshProUGUI p2_deckCountText;

    [Header("Round Wins UI")]
    [FormerlySerializedAs("p1_winsText")]
    public TextMeshProUGUI roundScoreText;
    private const float RoundWinsVisibleSeconds = 5f;
    private Coroutine hideRoundWinsRoutine;
    
    private HandSlotView[] p1HandSlots;
    private HandSlotView[] p2HandSlots;
    private Image[] p1CooldownFills;
    private Image[] p2CooldownFills;
    private Image p1DashCooldownFill;
    private Image p1ShieldCooldownFill;
    private Image p2DashCooldownFill;
    private Image p2ShieldCooldownFill;
    private CardCooldownNotification[] p1CooldownNotifications;
    private CardCooldownNotification[] p2CooldownNotifications;
    private CharacterCoordinator p1Character;
    private CharacterCoordinator p2Character;
    private bool p1DashWasReady;
    private bool p1ShieldWasReady;
    private bool p2DashWasReady;
    private bool p2ShieldWasReady;

    [Header("Card Placeholders")]
    [SerializeField] private Sprite emptySlotSprite;

    [Header("Card Reactivation Sound")]
    [SerializeField] private AudioClip cardReactivationSound;
    private AudioSource source;

    [Header("Damage Increase Sound")]

    [SerializeField] private AudioClip damageIncreaseSound;
    private static readonly Color CooldownColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [SerializeField] private AudioClip cardProgressionSound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        GameSettings.RegisterSource(source, GameSound.SoundEffects);

        p1CooldownFills = CreateCooldownFills(p1_cards);
        p2CooldownFills = CreateCooldownFills(p2_cards);
        p1DashCooldownFill = CreateCooldownFill(p1DashIcon);
        p1ShieldCooldownFill = CreateCooldownFill(p1ShieldIcon);
        p2DashCooldownFill = CreateCooldownFill(p2DashIcon);
        p2ShieldCooldownFill = CreateCooldownFill(p2ShieldIcon);
        p1DamageFlame = CreateDamageFlame(p1_damagePercent);
        p2DamageFlame = CreateDamageFlame(p2_damagePercent);
        HideRoundWins();
    }

    private void Start()
    {
        if (p1_energySlider != null) p1_energySlider.gameObject.SetActive(false);
        if (p2_energySlider != null) p2_energySlider.gameObject.SetActive(false);
        if (p1EnergyText != null) p1EnergyText.gameObject.SetActive(false);
        if (p2EnergyText != null) p2EnergyText.gameObject.SetActive(false);
        // Inicializa el HUD de daño al valor inicial (0).
        UpdateDamageUI(0, 0);
        UpdateDamageUI(1, 0);
        HideAllCardSlots();
        if (p1LastCardText != null) p1LastCardText.text = string.Empty;
        if (p2LastCardText != null) p2LastCardText.text = string.Empty;
        UpdateControlLabels();
        ApplyGameplayAids();
    }

    private void UpdateControlLabels()
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null)
            return;

        if (p1ControlLabel != null)
            p1ControlLabel.text = GetControlLabel(selection.matchMode, PlayerSlot.PlayerOne);
        if (p2ControlLabel != null)
            p2ControlLabel.text = GetControlLabel(selection.matchMode, PlayerSlot.PlayerTwo);
    }

    private static string GetControlLabel(MatchMode matchMode, PlayerSlot slot)
    {
        return ModeRules.GetControlMode(matchMode, slot) == PlayerMode.AI
            ? matchMode == MatchMode.AIVsAI
                ? (slot == PlayerSlot.PlayerOne ? "IA 1" : "IA 2")
                : "IA"
            : slot == PlayerSlot.PlayerOne ? "P1" : "P2";
    }

    private void OnEnable()
    {
        UIEvents.OnDamageChanged += UpdateDamageUI;
        UIEvents.OnLivesChanged += UpdateLivesUI;
        UIEvents.OnStockLost += PlayLostLifePulse;
        UIEvents.OnDeckCountChanged += UpdateDeckCountUI;
        UIEvents.OnIconSet += UpdateIconUI;
        UIEvents.OnHandChanged += UpdateHandUI;
        UIEvents.OnCardUsed += PlayCardUseAnimation;
        UIEvents.OnLastCardUsed += UpdateLastCardText;
        UIEvents.OnCardReward += AddACardOnFallDown;
        GameplayAidSettings.Changed += ApplyGameplayAids;
        ApplyGameplayAids();
    }

    private void OnDisable()
    {
        if (hideRoundWinsRoutine != null)
        {
            StopCoroutine(hideRoundWinsRoutine);
            hideRoundWinsRoutine = null;
        }

        p1CooldownNotifications = null;
        p2CooldownNotifications = null;
        UIEvents.OnDamageChanged -= UpdateDamageUI;
        UIEvents.OnLivesChanged -= UpdateLivesUI;
        UIEvents.OnStockLost -= PlayLostLifePulse;
        UIEvents.OnDeckCountChanged -= UpdateDeckCountUI;
        UIEvents.OnIconSet -= UpdateIconUI;
        UIEvents.OnHandChanged -= UpdateHandUI;
        UIEvents.OnCardUsed -= PlayCardUseAnimation;
        UIEvents.OnLastCardUsed -= UpdateLastCardText;
        UIEvents.OnCardReward -= AddACardOnFallDown;
        GameplayAidSettings.Changed -= ApplyGameplayAids;
    }

    private void Update()
    {
        AnimateDamageFlame(p1_damagePercent, p1DamageFlame, 0f);
        AnimateDamageFlame(p2_damagePercent, p2DamageFlame, 1.7f);
        UpdateCardCooldownVisuals(p1_cards, p1CooldownFills, p1HandSlots);
        UpdateCardCooldownVisuals(p2_cards, p2CooldownFills, p2HandSlots);
        if (GameplayAidSettings.ShowAbilityIcons)
            UpdateAbilityVisuals();

        if (Time.timeScale <= 0f)
            return;

        bool cardBecameReady = ConsumeCardReadyNotifications(p1_cards, p1HandSlots, ref p1CooldownNotifications);
        cardBecameReady |= ConsumeCardReadyNotifications(p2_cards, p2HandSlots, ref p2CooldownNotifications);
        // Simultaneous completions share one cue instead of stacking identical sounds.
        if (cardBecameReady && cardReactivationSound != null && source != null)
            source.PlayOneShot(cardReactivationSound);
    }

    private void UpdateDamageUI(int playerIndex, int damage)
    {
        TextMeshProUGUI percent = (playerIndex == 0) ? p1_damagePercent : p2_damagePercent;
        Image flame = playerIndex == 0 ? p1DamageFlame : p2DamageFlame;

        if (percent != null)
        {
            percent.text = $"{damage}";
            LeanTween.cancel(percent.gameObject);
            percent.color = Color.red;
            UpdateDamageFlame(percent, flame, damage >= DamageFlameThreshold);

            if (damage > 50 && damage <= 100 && source != null && damageIncreaseSound != null)
                source.PlayOneShot(damageIncreaseSound);

            // Efecto visual: pequeña escala cuando recibe daño
            if (damage > 0)
            {
                LeanTween.scale(percent.gameObject, new Vector3(1.05f, 1.05f, 1f), 0.4f)
                    .setEase(LeanTweenType.easeOutBounce)
                    .setOnComplete(() => {
                        LeanTween.scale(percent.gameObject, Vector3.one, 0.4f)
                            .setEase(LeanTweenType.easeInBounce);
                    });
            }
        }
    }

    private Image CreateDamageFlame(TextMeshProUGUI percent)
    {
        if (percent == null || damageFlameSprite == null)
            return null;

        GameObject flameObject = new GameObject("DamageFireCrown", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform textRect = percent.rectTransform;
        flameObject.transform.SetParent(textRect.parent, false);
        flameObject.layer = percent.gameObject.layer;

        RectTransform flameRect = (RectTransform)flameObject.transform;
        flameRect.anchorMin = textRect.anchorMin;
        flameRect.anchorMax = textRect.anchorMax;
        flameRect.pivot = textRect.pivot;
        flameRect.anchoredPosition = textRect.anchoredPosition + new Vector2(0f, 18f);
        // The flame renders behind the digits, with its tips rising above them.
        flameRect.SetSiblingIndex(textRect.GetSiblingIndex());

        Image flame = flameObject.GetComponent<Image>();
        flame.sprite = damageFlameSprite;
        flame.preserveAspect = true;
        flame.raycastTarget = false;
        flame.enabled = false;
        return flame;
    }

    private static void UpdateDamageFlame(TextMeshProUGUI percent, Image flame, bool visible)
    {
        if (flame == null)
            return;

        float width = Mathf.Clamp(percent.GetPreferredValues().x + 10f, 55f, 140f);
        float height = width * 0.5f;
        flame.rectTransform.sizeDelta = new Vector2(width, height);
        flame.rectTransform.anchoredPosition = percent.rectTransform.anchoredPosition + new Vector2(0f, 18f);
        if (flame.enabled != visible)
            flame.enabled = visible;
    }

    private static void AnimateDamageFlame(TextMeshProUGUI percent, Image flame, float phase)
    {
        if (percent == null || flame == null || !flame.enabled)
            return;

        float flicker = Mathf.Sin(Time.time * 12f + phase);
        flame.color = new Color(1f, 1f, 1f, 0.88f + 0.12f * flicker);
        flame.rectTransform.anchoredPosition = percent.rectTransform.anchoredPosition
            + new Vector2(0.8f * flicker, 18f + 1.5f * flicker);
        flame.rectTransform.localScale = Vector3.Scale(percent.rectTransform.localScale,
            new Vector3(1f + 0.03f * flicker, 1f + 0.07f * flicker, 1f));
    }

    private void UpdateLivesUI(int playerIndex, int lives)
    {
        GameObject[] icons = (playerIndex == 0) ? p1_life : p2_life;
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null) icons[i].SetActive(i < lives);
        }
        
    }

    private void PlayLostLifePulse(int playerIndex, int remainingLives)
    {
        GameObject[] icons = playerIndex == 0 ? p1_life : p2_life;
        if (remainingLives < 0 || remainingLives >= icons.Length)
            return;

        GameObject icon = icons[remainingLives];
        if (icon == null || !icon.activeInHierarchy || icon.transform.parent == null)
            return;

        UnityEngine.UI.Image source = icon.GetComponentInChildren<UnityEngine.UI.Image>();
        RectTransform sourceRect = source != null ? source.rectTransform : icon.GetComponent<RectTransform>();
        if (sourceRect == null)
            return;

        Canvas canvas = icon.GetComponentInParent<Canvas>();
        Transform ghostParent = canvas != null ? canvas.transform : icon.transform.parent;
        GameObject ghost = new GameObject("Lost Life Pulse", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(UnityEngine.UI.Image), typeof(LayoutElement));
        RectTransform rect = ghost.GetComponent<RectTransform>();
        rect.SetParent(ghostParent, false);
        rect.position = sourceRect.position;
        rect.rotation = sourceRect.rotation;
        rect.sizeDelta = sourceRect.rect.size;
        Vector3 parentScale = ghostParent.lossyScale;
        Vector3 sourceScale = sourceRect.lossyScale;
        rect.localScale = new Vector3(sourceScale.x / Mathf.Max(0.0001f, parentScale.x),
            sourceScale.y / Mathf.Max(0.0001f, parentScale.y), 1f);
        ghost.GetComponent<LayoutElement>().ignoreLayout = true;

        UnityEngine.UI.Image image = ghost.GetComponent<UnityEngine.UI.Image>();
        image.sprite = source != null ? source.sprite : null;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        StartCoroutine(AnimateLostLifePulse(ghost, image, PlayerColors.Get(playerIndex)));
    }

    private IEnumerator AnimateLostLifePulse(GameObject ghost, UnityEngine.UI.Image image, Color playerColor)
    {
        RectTransform rect = ghost.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        const float duration = 0.45f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (PauseMenu.isPaused)
            {
                ghost.SetActive(false);
                yield return null;
                continue;
            }

            if (!ghost.activeSelf)
                ghost.SetActive(true);
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            rect.localScale = originalScale * Mathf.Lerp(1f, 2.1f, progress);
            Color color = Color.Lerp(Color.white, playerColor, progress);
            color.a = 1f - progress;
            image.color = color;
            yield return null;
        }

        Destroy(ghost);
    }

    private void UpdateDeckCountUI(int playerIndex, int count)
    {
        TextMeshProUGUI text = (playerIndex == 0) ? p1_deckCountText : p2_deckCountText;
        if (text != null) text.text = count.ToString();
    }

    private void UpdateIconUI(int playerIndex, Sprite iconSprite)
    {
        Image icon = (playerIndex == 0) ? p1_icon : p2_icon;
        if (icon != null) icon.sprite = iconSprite;
    }

    private void UpdateLastCardText(int playerIndex, string cardName)
    {
        TextMeshProUGUI label = playerIndex == 0 ? p1LastCardText : p2LastCardText;
        if (label != null)
            label.text = cardName ?? string.Empty;
    }

    private void ApplyGameplayAids()
    {
        SetVisible(p1LastCardText, GameplayAidSettings.ShowLastCard);
        SetVisible(p2LastCardText, GameplayAidSettings.ShowLastCard);
        SetVisible(p1DashIcon, GameplayAidSettings.ShowAbilityIcons);
        SetVisible(p1ShieldIcon, GameplayAidSettings.ShowAbilityIcons);
        SetVisible(p2DashIcon, GameplayAidSettings.ShowAbilityIcons);
        SetVisible(p2ShieldIcon, GameplayAidSettings.ShowAbilityIcons);
    }

    private static void SetVisible(Component component, bool visible)
    {
        if (component != null && component.gameObject.activeSelf != visible)
            component.gameObject.SetActive(visible);
    }

    public void ShowRoundWinsForRoundResult()
    {
        if (hideRoundWinsRoutine != null)
            StopCoroutine(hideRoundWinsRoutine);

        SetVisible(roundScoreText, true);
        hideRoundWinsRoutine = StartCoroutine(HideRoundWinsAfterDelay());
    }

    private IEnumerator HideRoundWinsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(RoundWinsVisibleSeconds);
        SetVisible(roundScoreText, false);
        hideRoundWinsRoutine = null;
    }

    public void HideRoundWins()
    {
        if (hideRoundWinsRoutine != null)
        {
            StopCoroutine(hideRoundWinsRoutine);
            hideRoundWinsRoutine = null;
        }

        SetVisible(roundScoreText, false);
    }

    private void UpdateAbilityVisuals()
    {
        RespawnManager spawner = RespawnManager.Instance;
        GameObject p1Object = spawner != null ? spawner.p1Instance : null;
        GameObject p2Object = spawner != null ? spawner.p2Instance : null;

        BindCharacter(p1Object, ref p1Character, ref p1DashWasReady, ref p1ShieldWasReady);
        BindCharacter(p2Object, ref p2Character, ref p2DashWasReady, ref p2ShieldWasReady);

        UpdateAbilityIcons(p1Character, p1DashIcon, p1DashCooldownFill,
            p1ShieldIcon, p1ShieldCooldownFill,
            ref p1DashWasReady, ref p1ShieldWasReady);
        UpdateAbilityIcons(p2Character, p2DashIcon, p2DashCooldownFill,
            p2ShieldIcon, p2ShieldCooldownFill,
            ref p2DashWasReady, ref p2ShieldWasReady);
    }

    private static void BindCharacter(GameObject instance, ref CharacterCoordinator character,
        ref bool dashWasReady, ref bool shieldWasReady)
    {
        if (character != null && character.gameObject == instance)
            return;

        character = instance != null ? instance.GetComponent<CharacterCoordinator>() : null;
        dashWasReady = character != null && character.Dash != null && character.Dash.CanDash;
        shieldWasReady = character != null && character.Shield != null && character.Shield.CanActivate;
    }

    private static void UpdateAbilityIcons(CharacterCoordinator character, Image dashIcon, Image dashFill,
        Image shieldIcon, Image shieldFill,
        ref bool dashWasReady, ref bool shieldWasReady)
    {
        CharacterDash dash = character != null ? character.Dash : null;
        CharacterShield shield = character != null ? character.Shield : null;

        UpdateAbilityIcon(dashIcon, dashFill, dash != null && dash.CanDash,
            dash != null ? dash.CooldownRemaining : 0f,
            dash != null ? dash.CooldownDuration : 0f, ref dashWasReady);
        UpdateAbilityIcon(shieldIcon, shieldFill, shield != null && shield.CanActivate,
            shield != null ? shield.RemainingCooldown : 0f,
            shield != null ? shield.CooldownLength : 0f, ref shieldWasReady);
    }

    private static void UpdateAbilityIcon(Image icon, Image fill, bool ready, float remaining, float duration,
        ref bool wasReady)
    {
        if (icon == null)
            return;

        icon.color = ready ? Color.white : CooldownColor;
        // An active shield or dash has no running cooldown yet.
        SetCooldownFill(icon, fill, !ready && remaining > 0f && duration > 0f,
            remaining, duration);

        if (ready && !wasReady && icon.gameObject.activeInHierarchy)
        {
            LeanTween.cancel(icon.gameObject);
            icon.transform.localScale = Vector3.one;
            LeanTween.scale(icon.gameObject, Vector3.one * 1.25f, 0.16f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => LeanTween.scale(icon.gameObject, Vector3.one, 0.24f)
                    .setEase(LeanTweenType.easeOutBack));
        }

        wasReady = ready;
    }

    private void UpdateHandUI(int playerIndex, HandSlotView[] hand)
    {
        Image[] uiSlots = (playerIndex == 0) ? p1_cards : p2_cards;
        if (playerIndex == 0)
        {
            p1HandSlots = hand;
            SyncCooldownNotifications(hand, ref p1CooldownNotifications);
        }
        else
        {
            p2HandSlots = hand;
            SyncCooldownNotifications(hand, ref p2CooldownNotifications);
        }

        // Itera sobre TODOS los slots UI disponibles (no solo los de la mano)
        for (int i = 0; i < uiSlots.Length; i++)
        {
            Image slotImage = uiSlots[i];
            if (slotImage == null) continue;

            // Si el índice está fuera del rango de la mano o no está desbloqueado
            if (i >= hand.Length || !hand[i].IsUnlocked)
            {
                slotImage.gameObject.SetActive(false);
                continue;
            }

            // El slot está desbloqueado, lo mostramos
            slotImage.gameObject.SetActive(true);
            if (slotImage.transform.localScale == Vector3.zero)
                slotImage.transform.localScale = Vector3.one;

            // IMPORTANTE: Activar el Image antes de SetUI
            slotImage.enabled = true;

            RectTransform rect = slotImage.GetComponent<RectTransform>();

            if (hand[i].Card != null)
            {
                hand[i].Card.SetUI(slotImage);
                slotImage.color = Color.white;

            }
            else
            {
                slotImage.sprite = emptySlotSprite;
                slotImage.color = new Color(1f, 1f, 1f, 0.5f);

            }

            // Forzar reconstrucción del layout group padre
            if (rect.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect.parent as RectTransform);

            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();
        }

        UpdateCardCooldownVisuals(uiSlots,
            playerIndex == 0 ? p1CooldownFills : p2CooldownFills, hand);
    }

    private void HideAllCardSlots()
    {
        HideCardSlots(p1_cards);
        HideCardSlots(p2_cards);
    }

    public void HideCardsForTutorial()
    {
        HideAllCardSlots();
    }

    private static void HideCardSlots(Image[] slots)
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            slots[i].gameObject.SetActive(false);
            slots[i].transform.localScale = Vector3.zero;
        }
    }

    private static Image[] CreateCooldownFills(Image[] slots)
    {
        if (slots == null)
            return null;

        Image[] fills = new Image[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            Image slot = slots[i];
            if (slot == null)
                continue;

            fills[i] = CreateCooldownFill(slot);
        }

        return fills;
    }

    private static Image CreateCooldownFill(Image icon)
    {
        if (icon == null)
            return null;

        GameObject fillObject = new GameObject("CooldownFill", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(icon.transform, false);

        RectTransform rect = (RectTransform)fillObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image fill = fillObject.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Vertical;
        fill.fillOrigin = (int)Image.OriginVertical.Bottom;
        fill.raycastTarget = false;
        fill.enabled = false;
        return fill;
    }

    private static void SetCooldownFill(Image icon, Image fill, bool coolingDown,
        float remaining, float duration)
    {
        if (fill == null)
            return;

        if (!coolingDown)
        {
            fill.enabled = false;
            return;
        }

        fill.sprite = icon.sprite;
        fill.preserveAspect = icon.preserveAspect;
        fill.fillAmount = Mathf.Clamp01(1f - remaining / duration);
        fill.color = Color.white;
        fill.enabled = fill.sprite != null;
    }

    private static void UpdateCardCooldownVisuals(Image[] uiSlots, Image[] fills, HandSlotView[] hand)
    {
        if (uiSlots == null || fills == null)
            return;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            Image slotImage = uiSlots[i];
            Image fill = fills[i];
            if (slotImage == null || fill == null)
                continue;

            if (hand == null || i >= hand.Length || !slotImage.gameObject.activeSelf ||
                !hand[i].IsUnlocked || hand[i].Card == null)
            {
                fill.enabled = false;
                continue;
            }

            float remaining = hand[i].ReadyAt - Time.time;
            float duration = hand[i].CooldownDuration;
            if (remaining <= 0f || duration <= 0f)
            {
                slotImage.color = Color.white;
                fill.enabled = false;
                continue;
            }

            // Keep the complete card visible in the dark while its full-color image rises.
            slotImage.color = CooldownColor;
            SetCooldownFill(slotImage, fill, true, remaining, duration);
        }
    }



    private static void SyncCooldownNotifications(HandSlotView[] hand, ref CardCooldownNotification[] notifications)
    {
        if (hand == null)
        {
            notifications = null;
            return;
        }

        if (notifications == null || notifications.Length != hand.Length)
            notifications = new CardCooldownNotification[hand.Length];

        // Hand snapshots are reused by CharacterDeck, so each slot keeps its own identity/deadline.
        for (int i = 0; i < hand.Length; i++)
            notifications[i].Observe(hand[i], Time.time);
    }

    private static bool ConsumeCardReadyNotifications(Image[] images, HandSlotView[] hand,
        ref CardCooldownNotification[] notifications) 
    {
        SyncCooldownNotifications(hand, ref notifications);
        if (notifications == null)
            return false;

        bool becameReady = false;
        for (int i = 0; i < notifications.Length; i++)
        {
            bool completed = notifications[i].TryConsume(Time.time);
            if (completed && images != null && i < images.Length && images[i] != null &&
                images[i].gameObject.activeInHierarchy)
                becameReady = true;
        }
        return becameReady;
    }

    public void PlayCardUseAnimation(int playerIndex, int cardIndex)
    {
        Image[] cards = (playerIndex == 0) ? p1_cards : p2_cards;

        if (cardIndex >= 0 && cardIndex < cards.Length && cards[cardIndex] != null)
        {
            Image cardImage = cards[cardIndex];

            // Cancela cualquier animación anterior
            LeanTween.cancel(cardImage.gameObject);

            // Aplica la animación
            LeanTween.scale(cardImage.gameObject, Vector3.one * 3f, 0.2f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => {
                    LeanTween.scale(cardImage.gameObject, Vector3.one, 0.3f)
                        .setEase(LeanTweenType.easeOutQuint);
                });

            

            
        }
    }

    public void RevealRemainingCards(int playerIndex)
    {
        Image[] uiSlots = (playerIndex == 0) ? p1_cards : p2_cards;
        HandSlotView[] hand = (playerIndex == 0) ? p1HandSlots : p2HandSlots;
        if (hand == null)
            return;

        int startingHand = SelectionManager.Instance != null && !SelectionManager.Instance.isTrainingMode
            ? SelectionManager.Instance.Rules.InitialHandSize
            : 2;
        for (int i = startingHand; i < Mathf.Min(hand.Length, uiSlots.Length); i++)
        {
            if (hand[i].IsUnlocked && hand[i].Card != null)
            {
                uiSlots[i].gameObject.SetActive(true);
                hand[i].Card.SetUI(uiSlots[i]);

                uiSlots[i].transform.localScale = Vector3.zero;
                LeanTween.scale(uiSlots[i].gameObject, Vector3.one, 2.3f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setDelay(0.2f * (i - 2)); // escalonado
                source.PlayOneShot(cardProgressionSound);
            }
        }
    }


    public void AnimateInitialCards(int playerIndex)
    {
        Image[] uiSlots = (playerIndex == 0) ? p1_cards : p2_cards;
        HandSlotView[] hand = (playerIndex == 0) ? p1HandSlots : p2HandSlots;
        if (hand == null)
            return;

        int maxVisibleCards = SelectionManager.Instance != null && !SelectionManager.Instance.isTrainingMode
            ? SelectionManager.Instance.Rules.InitialHandSize
            : 2;
        maxVisibleCards = Mathf.Min(maxVisibleCards, hand.Length, uiSlots.Length);

        for (int i = 0; i < maxVisibleCards; i++)
        {
            if (!hand[i].IsUnlocked) continue;
            Image slotImage = uiSlots[i];
            if (slotImage == null) continue;

            slotImage.gameObject.SetActive(true);
            slotImage.transform.localScale = Vector3.zero;

            LeanTween.cancel(slotImage.gameObject);
            LeanTween.scale(slotImage.gameObject, Vector3.one * 1.2f, 0.3f)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() =>
                {
                    LeanTween.scale(slotImage.gameObject, Vector3.one, 0.2f)
                        .setEase(LeanTweenType.easeInOutQuad);
                });
        }
    }



    private void AddACardOnFallDown(int playerIndex)
    {
        RevealRemainingCards(playerIndex);
    }


    public void HideControlsImageAfterTheFirstTimeYouPlay()
    {
        if (controlsTutorial != null)
            controlsTutorial.gameObject.SetActive(false);
    }


}
