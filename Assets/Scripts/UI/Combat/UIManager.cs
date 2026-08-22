using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Character Icons")]
    public Image p1_icon;
    public Image p2_icon;

    [Header("Damage")]
    public Image p1_damageBar;
    public Image p2_damageBar;

    [Header("Cards P1")]
    public Image[] p1_cards = new Image[4];

    [Header("Cards P2")]
    public Image[] p2_cards = new Image[4];

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

    private static readonly Color CooldownColor = new Color(0.35f, 0.35f, 0.35f, 0.8f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        UIEvents.OnDamageChanged += UpdateDamageUI;
        UIEvents.OnLivesChanged += UpdateLivesUI;
        UIEvents.OnEnergyChanged += UpdateEnergyUI;
        UIEvents.OnDeckCountChanged += UpdateDeckCountUI;
        UIEvents.OnIconSet += UpdateIconUI;
        UIEvents.OnHandChanged += UpdateHandUI;
    }

    private void OnDisable()
    {
        UIEvents.OnDamageChanged -= UpdateDamageUI;
        UIEvents.OnLivesChanged -= UpdateLivesUI;
        UIEvents.OnEnergyChanged -= UpdateEnergyUI;
        UIEvents.OnDeckCountChanged -= UpdateDeckCountUI;
        UIEvents.OnIconSet -= UpdateIconUI;
        UIEvents.OnHandChanged -= UpdateHandUI;
    }

    private void Update()
    {
        UpdateCardCooldownVisuals(p1_cards, p1HandSlots);
        UpdateCardCooldownVisuals(p2_cards, p2HandSlots);
    }

    private void UpdateDamageUI(int playerIndex, int damage)
    {
        Image bar = (playerIndex == 0) ? p1_damageBar : p2_damageBar;
        if (bar != null)
        {
            if (damage < 25)
            {
                bar.GetComponent<Image>().color = Color.green;
            }

            else if (damage <= 50)
            {
                bar.GetComponent<Image>().color = Color.yellow;
            }

            else if (damage <= 75)
            {
                bar.GetComponent<Image>().color = Color.orange;
            }

            else if (damage <= 100)
            {
                bar.GetComponent<Image>().color = Color.red;
            }

            else
            {
                bar.GetComponent<Image>().color = Color.darkRed;
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

    private void UpdateEnergyUI(int playerIndex, int energy)
    {
        Slider slider = (playerIndex == 0) ? p1_energySlider : p2_energySlider;
        if (slider != null) slider.value = energy;

        TextMeshProUGUI text = (playerIndex == 0) ? p1EnergyText : p2EnergyText;
        if (text != null) text.text = energy.ToString();
    }

    private void UpdateDeckCountUI(int playerIndex, int count, int redrawsRemaining)
    {
        TextMeshProUGUI text = (playerIndex == 0) ? p1_deckCountText : p2_deckCountText;
        if (text != null) text.text = $"{count} | R:{redrawsRemaining}";
    }

    private void UpdateIconUI(int playerIndex, Sprite iconSprite)
    {
        Image icon = (playerIndex == 0) ? p1_icon : p2_icon;
        if (icon != null) icon.sprite = iconSprite;
    }

    private void UpdateHandUI(int playerIndex, HandSlotView[] hand)
    {
        Image[] uiSlots = (playerIndex == 0) ? p1_cards : p2_cards;
        if (playerIndex == 0)
            p1HandSlots = hand;
        else
            p2HandSlots = hand;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            bool hasVisibleCard = i < hand.Length && hand[i].IsUnlocked && hand[i].Card != null;
            if (hasVisibleCard)
            {
                uiSlots[i].gameObject.SetActive(true);
                hand[i].Card.SetUI(uiSlots[i]);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
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

            slotImage.color = Time.time >= hand[i].ReadyAt ? Color.white : CooldownColor;
        }
    }
}
