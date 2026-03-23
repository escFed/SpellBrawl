using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Throw Point")]
    [SerializeField] public Transform throwPoint;
 

    [Header("FireBall")]
    [SerializeField] GameObject fbPrefab;
    [SerializeField] GameObject fbCard;
    [SerializeField] private float cooldown;
    private Coroutine fbCoroutine;
    private bool canFbShot = true;
    private int maxFbShoots = 3;
    private int currentFbShoots;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;


    [Header("Input Responsible")]
    [SerializeField] GameObject hitBox;
    [SerializeField] Coroutine coroutine;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isGrounded;

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Fire.performed += OnFire;
      

        currentFbShoots = maxFbShoots;

    }

    private void OnDestroy()
    {
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Fire.performed -= OnFire;
      
        
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        
       

   
         
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

 
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }


    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (!fbCard.activeSelf) return;
        if(!canFbShot) return;
        if(currentFbShoots <= 0) return; 

        canFbShot = false;

        GameObject fireball = Instantiate(fbPrefab, throwPoint.position, Quaternion.identity);

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        fireball.GetComponent<FireBallScript>().Init(direction);
      

        currentFbShoots--;

        if(fbCoroutine != null)
        {
            StopCoroutine(fbCoroutine);
        }

        if(currentFbShoots <= 0)
        {
            canFbShot = false;
            fbCard.SetActive(false);
        }


        fbCoroutine = StartCoroutine(CardDeactivated());

        StartCoroutine(FbShoot());
        
    }



    private IEnumerator CardDeactivated()
    {
        float time = 0f;
        float duration = 6f;

        Vector3 startPos = fbCard.transform.position;
        Vector3 targetPos = startPos + new Vector3(-1f, 3f, 0f);

        while (time < duration)
        {
            time += Time.deltaTime;

            fbCard.transform.position = Vector3.Lerp(startPos, targetPos, time / duration);

            yield return null;
        }

        fbCard.SetActive(false);
      



    }
    private IEnumerator FbShoot()
    {
        yield return new WaitForSeconds(cooldown);
        canFbShot = true;
    }




}
