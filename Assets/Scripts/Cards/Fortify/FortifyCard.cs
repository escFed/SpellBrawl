using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class FortifyCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    private Image img;
    private bool canFort = true;
    [SerializeField] private GameObject fortPrefab;

    public void SetUI(Image image)
    {
        img = image;
    }

    public void ExecuteCard(PlayerController controller)
    {
        if (!canFort) return;


        {
            Debug.Log("Executing Fortify Card");
            FortifyLogic fortify = Instantiate(fortPrefab).GetComponent<FortifyLogic>();
            fortify.Init(controller.gameObject);
            fortify.FortifyExecution(fortify.damageReduction, controller.PlayerId);
        }
    }
}
