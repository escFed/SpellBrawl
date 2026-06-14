using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;


    public IEnumerator MirrorWorldActivated()
    {

        if(playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            playerController.Movement.moveSpeedMultiplier = -1f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerController.Movement.moveSpeedMultiplier = 1f;
    }
}
