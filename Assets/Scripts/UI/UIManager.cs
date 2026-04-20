using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Damage")]
    public TextMeshProUGUI p1_damageText;
    public TextMeshProUGUI p2_damageText;

    [Header("Player 1")]
    public Image[] p1_cards = new Image[4];

    [Header("Player 2")]
    public Image[] p2_cards = new Image[4];

    [Header("Life")]
    public GameObject[] p1_life = new GameObject[3];
    public GameObject[] p2_life = new GameObject[3];

   

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void UpdateDamage(int playerId, int damage, float knockBackReductionQuantity)
    {
        TextMeshProUGUI damageUI = (playerId == 1) ? p1_damageText : p2_damageText;

        if (damageUI != null)
        {
            // Mostrar solo daño
            damageUI.text = damage + "%";

            
        }
    }



   



}
