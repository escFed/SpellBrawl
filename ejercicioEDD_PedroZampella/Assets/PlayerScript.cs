using Unity.VisualScripting;
using UnityEngine;
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BulletScript b;
    [SerializeField] public Transform throwPoint;
   


    private void Start()
    {
       if(b == null)
        {
            b = GetComponent<BulletScript>();
        }
    }


    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {


            Vector3 direction = Vector3.up;
            Vector3 spawnPos = new Vector3(throwPoint.position.x, throwPoint.position.y, throwPoint.position.z);

            b.Shoot(direction, spawnPos);

            Debug.Log("Shoot");

        }
    }


    private void FixedUpdate()
    {
        Movement();


    }
    private void Movement()
    {

        rb = gameObject.GetComponent<Rigidbody>();
        float x = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(x, 0, 0) * speed * Time.fixedDeltaTime ;
        rb.MovePosition(rb.position + movement);

        
    }
}
