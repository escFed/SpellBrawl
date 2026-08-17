using System.Collections;
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

    private IEnumerator animateColorCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Auto-asignar barras de daño si no están asignadas
        if (p1_damageBar == null)
        {
            p1_damageBar = GameObject.Find("P1_damage Bar")?.GetComponent<Image>();
            if (p1_damageBar == null)
                p1_damageBar = GameObject.Find("P1DamageBar")?.GetComponent<Image>();
            if (p1_damageBar == null)
                p1_damageBar = FindImageByName("P1_damage");
        }

        if (p2_damageBar == null)
        {
            p2_damageBar = GameObject.Find("P2_damage Bar")?.GetComponent<Image>();
            if (p2_damageBar == null)
                p2_damageBar = GameObject.Find("P2DamageBar")?.GetComponent<Image>();
            if (p2_damageBar == null)
                p2_damageBar = FindImageByName("P2_damage");
        }

        // Asegurar que las barras sean visibles con un color inicial
        if (p1_damageBar != null)
        {
            p1_damageBar.color = Color.green;
            Debug.Log($"✓ P1 damage bar encontrada: {p1_damageBar.name}");
        }
        else
            Debug.LogError("✗ P1 damage bar NO encontrada!");

        if (p2_damageBar != null)
        {
            p2_damageBar.color = Color.green;
            Debug.Log($"✓ P2 damage bar encontrada: {p2_damageBar.name}");
        }
        else
            Debug.LogError("✗ P2 damage bar NO encontrada!");
    }

    private Image FindImageByName(string searchTerm)
    {
        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (var img in allImages)
        {
            if (img.name.Contains(searchTerm))
                return img;
        }
        return null;
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
        Image bar = (playerIndex == 0) ? p1_damageBar : p2_damageBar;
        if (bar == null) return;

        Color targetColor = GetDamageColor(damage);

        // Cancelar animaciones previas
        if (animateColorCoroutine != null)
            StopCoroutine(animateColorCoroutine);

        // Animar solo el color, la barra ya está completa
        animateColorCoroutine = AnimateBarColor(bar, targetColor, 0.5f);
        StartCoroutine(animateColorCoroutine);
    }

    private IEnumerator AnimateBarColor(Image bar, Color targetColor, float duration)
    {
        if (bar == null) yield break;

        Color startColor = bar.color;
        float elapsed = 0f;

        while (elapsed < duration && bar != null)
        {
            elapsed += Time.deltaTime;
            bar.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        if (bar != null)
            bar.color = targetColor;
    }

    private Color GetDamageColor(int damage)
    {
        if (damage < 25) return Color.green;
        if (damage <= 50) return Color.yellow;
        if (damage <= 75) return Color.red;
        if (damage <= 100) return new Color(1f, 0.5f, 0f);  // Orange
        return new Color(0.4f, 0f, 0f);  // Dark red
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
