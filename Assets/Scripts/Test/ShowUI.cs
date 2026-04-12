using System.Collections;
using UnityEngine;

public class ShowUI : MonoBehaviour
{
    public float WaitingTime = 5f;

    void Start()
    {
        StartCoroutine(DeactivateAfterTime());
    }

    IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(WaitingTime);

        gameObject.SetActive(false);
    }
}
