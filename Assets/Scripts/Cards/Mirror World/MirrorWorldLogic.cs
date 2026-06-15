using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;


    public void Initialize(PlayerController ctrl)
    {
        playerController = ctrl;
    }
    public IEnumerator MirrorWorldActivated()
    {

    
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            playerController.Movement.moveSpeedMultiplier = -1f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerController.Movement.moveSpeedMultiplier = 1f;


        Destroy(gameObject, 0.5f);
    }
}
