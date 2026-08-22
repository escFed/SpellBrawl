using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;
    private PlayerController targetController;


    public void Initialize(PlayerController ctrl, PlayerController target)
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

        yield return new WaitForSeconds(effectDuration > 0f ? effectDuration : 3f);
        Destroy(gameObject);
    }
}

