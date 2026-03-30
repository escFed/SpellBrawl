using UnityEngine;
using UnityEngine.UI;

public class CardUIManager : MonoBehaviour
{
    public static CardUIManager Instance;

    [Header("Player 1 UI (Izquierda)")]
    public Image p1_fireCard;
    public Image p1_thunderCard;

    [Header("Player 2 UI (Derecha)")]
    public Image p2_fireCard;
    public Image p2_thunderCard;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
