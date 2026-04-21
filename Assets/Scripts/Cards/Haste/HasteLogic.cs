using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
public class HasteLogic : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int newDamageSpeed;
    [SerializeField] private float newMovementSpeed;
    [SerializeField] private float currentTime;
    [SerializeField] private float timeToReset = 3f;
    private GameObject targeter;
    private bool hasteActive = false;
    private PlayerController controller;


    public void Init(GameObject target)
    {
        targeter = target;
    }

    // Update is called once per frame
    public void HasteExecution(int playerId)
    {
          controller = targeter.GetComponent<PlayerController>();

        if (controller.PlayerId != playerId) return;
        {
            
            controller.Stats.moveSpeed = controller.Stats.moveSpeed * newMovementSpeed;
            controller.TakeDamage(newDamageSpeed, new Vector2(0f, 0f));

            currentTime = 0f;
            hasteActive = true;
            
        }
    }

    public void Update()
    {
        if (!hasteActive) return;

        currentTime += Time.deltaTime;

        if (currentTime >= timeToReset)
        {

            controller.Stats.moveSpeed = 5f;
            DamageManager.CalculateDamage(10);
            hasteActive = false;
        }

    }
}
