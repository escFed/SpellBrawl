using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private PlayerInput keyBoardInput;
    [SerializeField] private Rigidbody rb;
    private Vector3 inputK;
    [SerializeField] private float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    rb = GetComponent<Rigidbody>();
    keyBoardInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
    
        inputK = keyBoardInput.actions["MoveKeyBoard"].ReadValue<Vector3>();
        
        
    }

    private void FixedUpdate()
    {
        if(inputK != Vector3.zero)
        {
            rb.linearVelocity = inputK * speed * Time.deltaTime;
        }

        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }



}
