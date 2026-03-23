using UnityEngine;

public class FireBallScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private PlayerController p;
    private Rigidbody2D rb;

    void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("AICharacter"))
        {
            
            Destroy(gameObject);
            
        }

        if(collision.gameObject.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }


    
}
