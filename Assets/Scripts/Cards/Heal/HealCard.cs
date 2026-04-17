using UnityEngine;
using UnityEngine.UI;

public class HealCard : MonoBehaviour, ICardable
{

    [Header("Settings")]
    private Image cardUI;
    private bool canHeal = true;
    [SerializeField] private Vector2 knockBackRed;
    [SerializeField] private float duration;
    [SerializeField] private GameObject hPrefab;

    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
    }

    public void ExecuteCard(PlayerController player)
    {
        if (!canHeal) return;

        Debug.Log("Executing Heal Card");


        HealLogic logic = Instantiate(hPrefab).GetComponent<HealLogic>();
        logic.Init(player.gameObject);
        logic.HealExecution(player.PlayerId);
    }




  
}
