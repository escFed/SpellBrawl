using UnityEngine;

public class TsunamiWave : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] public int damage = 10;
    [SerializeField] public Vector2 knockback = new Vector2(15f, 5f);
    [SerializeField] private float speed = 10f;
    [SerializeField] public float hitStun = 0.2f;
    [SerializeField] private int attackerPlayerIndex = -1;

     private float lifeTime;


    private GameObject caster;

    private Transform target;

    public void Init(GameObject casterObject, Transform targetTransform, float waveLifeTime)
    {
        caster = casterObject;
        target = targetTransform;
        lifeTime = waveLifeTime;
        Destroy(gameObject, lifeTime);
    }
   
    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (caster != null && collision.transform.root == caster.transform.root) return;

        ICombatHitReceiver hitTarget = collision.GetComponentInParent<ICombatHitReceiver>();
        if (hitTarget != null)
        {
            Vector2 knockbackDirection = (collision.transform.position - caster.transform.position).normalized;
            Vector2 adjustedKnockback = knockbackDirection * knockback.magnitude;
            hitTarget.ReceiveHit(new CombatHit(damage, adjustedKnockback, hitStun, HitReaction.StrongHit, collision.ClosestPoint(transform.position), attackerPlayerIndex));
            Destroy(gameObject);
        }
    }

}
