using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize(PlayerController player)
    {
        playerController = player;
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
    }
}
