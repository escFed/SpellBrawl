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
    public TextMeshProUGUI p1_damageText;
    public TextMeshProUGUI p2_damageText;

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

    private void UpdateDamageUI(int playerIndex, int damage)
    {
        TextMeshProUGUI text = (playerIndex == 0) ? p1_damageText : p2_damageText;
        if (text != null)
        {
            text.text = damage + "%";
            text.color = damage >= 100 ? Color.red : Color.white;
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

    private void UpdateHandUI(int playerIndex, ICardable[] hand)
    {
        Image[] uiSlots = (playerIndex == 0) ? p1_cards : p2_cards;
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < hand.Length && hand[i] != null)
            {
                uiSlots[i].gameObject.SetActive(true);
                hand[i].SetUI(uiSlots[i]);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
