using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HealLogic : MonoBehaviour
{
    [Header("Stats")]
   
   [SerializeField] private float timeForHealActivated;
    [SerializeField] private int damage;
   
    private GameObject targeter;



    public void Init(GameObject target)
    {
        targeter = target;
    }

    public void HealExecution(int playerId)
{
        UIManager.Instance.UpdateDamage(playerId, damage, timeForHealActivated);

    // Mostrar feedback visual SOLO aquí
    string message = $"Damage ↓ {damage}%";

        DamageManager.UpdateTargetText(playerId, message);
}



}
