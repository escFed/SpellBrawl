using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private CharacterCoordinator playerController;
    private CharacterCoordinator targetController;


    public void Initialize(CharacterCoordinator ctrl, CharacterCoordinator target)
    {
        playerController = ctrl;

        targetController = target;
    }

    public void Activate()
    {
        StartCoroutine(MirrorWorldActivated());
    }

    private IEnumerator MirrorWorldActivated()
    {
        if (targetController == null)
        {
            Destroy(gameObject);
            yield break;
        }

        if (targetController != null)
        {
            CharacterMovement targetMovement = targetController.GetComponent<CharacterMovement>();
            if (targetMovement != null)
            {
                targetMovement.moveSpeedMultiplier *= -1;
                yield return new WaitForSeconds(effectDuration > 0f ? effectDuration : 3f);
                Destroy(gameObject);
            }
        }
    }
}

