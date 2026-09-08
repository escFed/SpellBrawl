using UnityEngine;

public class TsunamiWave : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] public int knockBackAmount = 35;
    [SerializeField] private float speed = 10f;

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
        if(collision.gameObject == caster) return;

        if(collision.TryGetComponent(out HitReaction hitTarget))
        {
            Vector2 knockbackDirection = (collision.transform.position - caster.transform.position).normalized;
            Destroy(gameObject);
        }
    }
}
