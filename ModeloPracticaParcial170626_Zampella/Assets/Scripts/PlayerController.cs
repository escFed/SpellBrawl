using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private PlayerInput input;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float mSpeed = 5.0f;
    [SerializeField] private float jForce = 2.0f;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip jClip;
    private Vector2 inputVector;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (input != null) {

            inputVector = input.actions["Move"].ReadValue<Vector2>();

        }
    }


    void FixedUpdate()
    {
        Vector3 move = new Vector3(inputVector.x, 0, inputVector.y);
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }


    public void Jump(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            rb.AddForce(Vector3.up * jForce, ForceMode.Impulse);
            source.PlayOneShot(jClip);
        }
    }
}
