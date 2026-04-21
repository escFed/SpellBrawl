using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    private bool canHaste = true;
    [SerializeField] private GameObject hstPrefab;
    public Sprite cardSprite;
    private Image cardUI;
    public int EnergyCost => 40;

    public void SetUI(Image image) 
    { 
        this.cardUI = image;
    }
   public void ExecuteCard(PlayerController p)
    {
        if (!canHaste) return;

        Debug.Log("Executing Haste Card");
        HasteLogic logic = Instantiate(hstPrefab).GetComponent<HasteLogic>();
        logic.Init(p.gameObject);
        logic.HasteExecution(p.PlayerId);
    }


}
