using System.Collections;
using UnityEngine;

public class BlackHoleLogic : MonoBehaviour
{
    [SerializeField] public float effectDuration = 1.0f;

    private PlayerController targetController;

    public void Initialize(PlayerController trgt)
    {
        targetController = trgt;
    }

    public IEnumerator HoleRoutine()
    {
        if (targetController == null)
        {
            Debug.LogError("No target in Black Hole");
        }

        yield return new WaitForSeconds(effectDuration);
        Destroy(gameObject);
    }
}
