using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
     
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
