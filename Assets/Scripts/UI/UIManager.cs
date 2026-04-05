using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Player 1")]
    public Image p1_fireCard;
    public Image p1_thunderCard;

    [Header("Player 2")]
    public Image p2_fireCard;
    public Image p2_thunderCard;

    [Header("Damage UI")]
    public TextMeshProUGUI p1_damageText;
    public TextMeshProUGUI p2_damageText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
