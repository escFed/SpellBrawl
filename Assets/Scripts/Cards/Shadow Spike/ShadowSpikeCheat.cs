using System.Collections;
using UnityEngine;

public class ShadowSpikeCheat : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage = 25;
    [SerializeField] private Vector2 knockback = new Vector2(0f, 0f);
    [SerializeField] private float delayBetweenAnimations;
    [SerializeField] private GameObject shadowSpikePrefab;
    [SerializeField] private float delayForFirstFrame;
    [SerializeField] private LayerMask shadowLayer;
    private GameObject caster;
    private Transform target;
    private Animator anim;
    private CameraShake shake;



    public void Init(GameObject casterObject, Transform targetTransform)
    {
        caster = casterObject;
        target = targetTransform;

        anim = GetComponent<Animator>();

        shake = Camera.main.GetComponent<CameraShake>();

        StartCoroutine(ShadowAppear());


    }






    private IEnumerator ShadowAppear()
    {

        if (anim != null)
        {
            anim.SetTrigger("SpikeStart");
        }

        StartCoroutine(shake.Shake(delayForFirstFrame, 0.00001f));

        yield return new WaitForSeconds(delayForFirstFrame);

        Instantiate(shadowSpikePrefab, target.position, Quaternion.identity);

        yield return new WaitForSeconds(5f);
        Destroy(gameObject);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject == caster) return;

        if (collision.TryGetComponent(out IDamageable hitTarget))
        {
            int finalDamage = DamageManager.CalculateDamage(damage);
            Vector2 finalKnockback = DamageManager.CalculateKnockback(hitTarget.GetPlayerId(), knockback);
            hitTarget.TakeDamage(finalDamage, finalKnockback);
            Destroy(gameObject);
        }

        else if (((1 << collision.gameObject.layer) & shadowLayer) != 0)
        {
            Destroy(gameObject);
        }
    }

}

