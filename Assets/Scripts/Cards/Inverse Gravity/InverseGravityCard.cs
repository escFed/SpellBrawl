using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InverseGravityCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    private Image cardUi;
    public bool isInverted = false;
    [SerializeField] private GameObject iGprefab;

    public void SetUI(Image uiImage)
    {
        cardUi = uiImage;
    }


    public void ExecuteCard(PlayerController player)
    {
        if (!isInverted) return; 


        Debug.Log("Executing Inverse Gravity Card for Player " + player.PlayerId);

        InverseGravityLogic logic = player.gameObject.AddComponent<InverseGravityLogic>();
        StartCoroutine(logic.GravityInversion(player, logic.duration));

    }
}
