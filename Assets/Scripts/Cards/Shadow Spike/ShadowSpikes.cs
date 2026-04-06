using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowSpikes : MonoBehaviour
{
    [SerializeField] public int damageAmount;
    [SerializeField] public Transform[] spawnPos = new Transform[5];
    [SerializeField] public float delay;
    [SerializeField] public GameObject aShSpPrefab;


    void Start()
    {
        if (aShSpPrefab == null)
        {
            aShSpPrefab = this.GetComponent<GameObject>();
        }

        

    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("AICharacter"))
        {
            if (collision.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damageAmount, Vector2.zero);
            }
        }
    }
}
