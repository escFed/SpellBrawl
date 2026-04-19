using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HealLogic : MonoBehaviour
{
    [Header("Stats")]
   
   [SerializeField] private float timeForHealActivated;
    [SerializeField] private Vector2 knockBackReduction;
   
    private GameObject targeter;



    public void Init(GameObject target)
    {
        targeter = target;
    }

    public void HealExecution(int playerId)
{
    DamageManager.AddKnockbackReduction(playerId, knockBackReduction, timeForHealActivated);

    // Mostrar feedback visual SOLO aquí
    string message = $"Knockback ↓ {knockBackReduction}%";
        UIManager.Instance.ShowKnockbackBuff(playerId, timeForHealActivated);
        DamageManager.UpdateTargetText(playerId, message);
}



}
