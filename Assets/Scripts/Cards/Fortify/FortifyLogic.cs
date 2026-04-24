using UnityEngine;

public class FortifyLogic : MonoBehaviour
{
    [SerializeField] public int damageReduction;
    [SerializeField] private int fortifyDuration;
    private GameObject target;
    private PlayerController controller;
    public void Init(GameObject target)
    {
        this.target = target;
    }



    public void FortifyExecution(int damage, int playerId)
    {
        controller = target.GetComponent<PlayerController>();

        if (controller.PlayerId != playerId) return;
        {
            damage = damageReduction;
            DamageManager.AddGlobalDamageReduction(damage, fortifyDuration);
            controller.TakeDamage(damage, new Vector2(0f, 0f));
        }
     

    }
}
