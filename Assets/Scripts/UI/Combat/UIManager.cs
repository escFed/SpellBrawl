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

    [Header("Damage")]
    public TextMeshProUGUI p1_damagePercent;
    public TextMeshProUGUI p2_damagePercent;

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
    public TMPro.TextMeshProUGUI p1_winsText;
    public TMPro.TextMeshProUGUI p2_winsText;
    
    private HandSlotView[] p1HandSlots;
    private HandSlotView[] p2HandSlots;
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
    private static readonly Color CooldownColor = new Color(0.35f, 0.35f, 0.35f, 0.8f);

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
    }

    private void Start()
    {
        if (p1_energySlider != null) p1_energySlider.gameObject.SetActive(false);
        if (p2_energySlider != null) p2_energySlider.gameObject.SetActive(false);
        if (p1EnergyText != null) p1EnergyText.gameObject.SetActive(false);
        if (p2EnergyText != null) p2EnergyText.gameObject.SetActive(false);
        // Inicializa el color de las barras de daño al valor inicial (0)
        UpdateDamageUI(0, 0);
        UpdateDamageUI(1, 0);
        HideAllCardSlots();
        if (p1LastCardText != null) p1LastCardText.text = string.Empty;
        if (p2LastCardText != null) p2LastCardText.text = string.Empty;
        ApplyGameplayAids();
    }

    private void OnEnable()
    {
        UIEvents.OnDamageChanged += UpdateDamageUI;
        UIEvents.OnLivesChanged += UpdateLivesUI;
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
        p1CooldownNotifications = null;
        p2CooldownNotifications = null;
        UIEvents.OnDamageChanged -= UpdateDamageUI;
        UIEvents.OnLivesChanged -= UpdateLivesUI;
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
        UpdateCardCooldownVisuals(p1_cards, p1HandSlots);
        UpdateCardCooldownVisuals(p2_cards, p2HandSlots);
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

        if (percent != null)
        {
            percent.text = $"{damage}";
            LeanTween.cancel(percent.gameObject);

            // Determina el color objetivo según el daño
            Color targetColor;
            if (damage < 25)
            { 
                targetColor = Color.green;
                
            }
            else if (damage <= 50)
            {
                targetColor = Color.yellow;
                
            }
            else if (damage <= 75)
            {
                targetColor = new Color(1f, 0.5f, 0f); // Naranja

                if(source != null && damageIncreaseSound != null)
                source.PlayOneShot(damageIncreaseSound);
            }
            else if (damage <= 100)
            {
                targetColor = Color.red;
                if(source != null && damageIncreaseSound != null)
                source.PlayOneShot(damageIncreaseSound);
            }
            else
            {
                targetColor = Color.darkRed;
                
            }

            // Anima el color suavemente
            LeanTween.value(percent.gameObject, percent.color, targetColor, 1.2f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((Color c) => percent.color = c);

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

    private void UpdateLivesUI(int playerIndex, int lives)
    {
        GameObject[] icons = (playerIndex == 0) ? p1_life : p2_life;
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null) icons[i].SetActive(i < lives);
        }
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

    private void UpdateAbilityVisuals()
    {
        RespawnManager spawner = RespawnManager.Instance;
        GameObject p1Object = spawner != null ? spawner.p1Instance : null;
        GameObject p2Object = spawner != null ? spawner.p2Instance : null;

        BindCharacter(p1Object, ref p1Character, ref p1DashWasReady, ref p1ShieldWasReady);
        BindCharacter(p2Object, ref p2Character, ref p2DashWasReady, ref p2ShieldWasReady);

        UpdateAbilityIcons(p1Character, p1DashIcon, p1ShieldIcon,
            ref p1DashWasReady, ref p1ShieldWasReady);
        UpdateAbilityIcons(p2Character, p2DashIcon, p2ShieldIcon,
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

    private static void UpdateAbilityIcons(CharacterCoordinator character, Image dashIcon, Image shieldIcon,
        ref bool dashWasReady, ref bool shieldWasReady)
    {
        CharacterDash dash = character != null ? character.Dash : null;
        CharacterShield shield = character != null ? character.Shield : null;

        UpdateAbilityIcon(dashIcon, dash != null && dash.CanDash,
            dash != null ? dash.CooldownRemaining : 0f,
            dash != null ? dash.CooldownDuration : 0f, ref dashWasReady);
        UpdateAbilityIcon(shieldIcon, shield != null && shield.CanActivate,
            shield != null ? shield.RemainingCooldown : 0f,
            shield != null ? shield.CooldownLength : 0f, ref shieldWasReady);
    }

    private static void UpdateAbilityIcon(Image icon, bool ready, float remaining, float duration,
        ref bool wasReady)
    {
        if (icon == null)
            return;

        float progress = ready ? 1f : duration > 0f
            ? LeanTween.easeInOutQuad(0f, 1f, Mathf.Clamp01(1f - remaining / duration))
            : 0f;
        // An active shield and the final dash in progress have no running cooldown yet.
        if (!ready && remaining <= 0f)
            progress = 0f;
        icon.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.25f, 1f, progress));

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
            if (i >= 2 && slotImage.transform.localScale == Vector3.zero)
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

    private static void UpdateCardCooldownVisuals(Image[] uiSlots, HandSlotView[] hand)
    {
        if (uiSlots == null || hand == null)
            return;

        int count = Mathf.Min(uiSlots.Length, hand.Length);

        for (int i = 0; i < count; i++)
        {
            Image slotImage = uiSlots[i];
            if (slotImage == null || !slotImage.gameObject.activeSelf || hand[i].Card == null)
                continue;

            float remaining = hand[i].ReadyAt - Time.time;
            if (remaining <= 0f)
            {
                // Carta lista: alpha completo
                slotImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // Cooldown activo: alpha se va llenando
                float totalCooldown = hand[i].CooldownDuration;
                if (totalCooldown <= 0f)
                {
                    slotImage.color = new Color(1f, 1f, 1f, 0.5f);
                    continue;
                }

                float raw = 1f - (remaining / totalCooldown);
                raw = Mathf.Clamp01(raw);

                // Usar easing con start=0, end=1 y val=raw
                float progress = LeanTween.easeInOutQuad(0f, 1f, raw);

                slotImage.color = new Color(1f, 1f, 1f, progress);
            }
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

        for (int i = 2; i < hand.Length; i++)
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
        int maxVisibleCards = 2;

        for (int i = 0; i < maxVisibleCards; i++)
        {
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
