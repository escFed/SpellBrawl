using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Throw Point")]
    [SerializeField] public Transform throwPoint;


    [Header("StarThrow")]
    [SerializeField] GameObject sThPrefab;
    [SerializeField] public Transform starThrowPoint;

    [Header("FireBall")]
    [SerializeField] GameObject fbPrefab;
    [SerializeField] GameObject fbCard;
    [SerializeField] private float cooldown;
    private Coroutine fbCoroutine;
    private bool canFbShot = true;
    private int maxFbShoots = 3;
    private int currentFbShoots;


    [Header("ThunderStrike")]

    [SerializeField] GameObject tsPrefab;
    [SerializeField] GameObject tsCard;
    [SerializeField] ThunderStrikeScript tsScript;
    //[SerializeField] public Transform thunderPoint;
    [SerializeField] private float cooldown2;
    [SerializeField] private float currentThunderTime;
    private bool canThunder = true;
    private Coroutine thunderCoroutine;



    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;



    [Header("AI")]
    [SerializeField] private GameObject e;

    [Header("Input Responsible")]
    [SerializeField] GameObject hitBox;
    [SerializeField] Coroutine coroutine;

    public Rigidbody2D rb;
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
        inputActions.Player.Thunder.performed += OnThunder;
        inputActions.Player.StarThrow.performed += OnStarThrow;
        currentFbShoots = maxFbShoots;

    }

    private void OnDestroy()
    {
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Fire.performed -= OnFire;
        inputActions.Player.Thunder.performed -= OnThunder;
        inputActions.Player.StarThrow.performed -= OnStarThrow;


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
        if (!canFbShot) return;
        if (currentFbShoots <= 0) return;

        canFbShot = false;

        GameObject fireball = Instantiate(fbPrefab, throwPoint.position, Quaternion.identity);

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        fireball.GetComponent<FireBallScript>().Init(direction);


        currentFbShoots--;

        if (currentFbShoots > 0)
        {
            canFbShot = true;
        }

        else
        {

            fbCard.SetActive(false);

            if (fbCoroutine != null)
            {
                StopCoroutine(fbCoroutine);
            }


            fbCoroutine = StartCoroutine(CardDeactivated());
        }
        StartCoroutine(FbShoot());

    }
    private void OnStarThrow(InputAction.CallbackContext ctx)
    {


        // Distancia mínima y máxima
        float minDistance = 0f;
        float maxDistance = 20f;

        EnemyController enemy = e.GetComponent<EnemyController>();
        if (enemy == null) return;

        Vector2 direction = (enemy.transform.position - starThrowPoint.position).normalized;
        float distanceToEnemy = Vector2.Distance(starThrowPoint.position, enemy.transform.position);





        Debug.DrawLine(starThrowPoint.position, starThrowPoint.position + (Vector3)direction * maxDistance, Color.red);

        if (distanceToEnemy >= minDistance && distanceToEnemy <= maxDistance)
        {

            // Raycast desde el punto de disparo
            RaycastHit2D hit = Physics2D.Raycast(starThrowPoint.position, direction, maxDistance);
            Vector2 preciseDirection;


            if (hit.collider != null && hit.collider.CompareTag("AICharacter"))
            {

                preciseDirection = (hit.point - (Vector2)starThrowPoint.position).normalized;



            }

            else
            {
                preciseDirection = direction;
            }

            GameObject star = Instantiate(sThPrefab, starThrowPoint.position, Quaternion.identity);

            star.GetComponent<StarThrowScript>().Init(preciseDirection);



        }



    }

    private IEnumerator CardDeactivated()
    {

        float culldown = 6f;
        SpriteRenderer cardRenderer = fbCard.GetComponent<SpriteRenderer>();

        canFbShot = false;
        fbCard.SetActive(false);

        yield return new WaitForSeconds(culldown);

        canFbShot = true;
        fbCard.SetActive(true);

        cardRenderer.color = Color.white;

        currentFbShoots = 3;



    }


       

        


    
    private IEnumerator FbShoot()
    {
        yield return new WaitForSeconds(cooldown);
        canFbShot = true;
    }


    private void OnThunder(InputAction.CallbackContext ctx)
    {
        // Solo permitir si la carta está activa y se puede usar
        if (tsCard.activeSelf && canThunder)
        {
            StartCoroutine(ThunderAnimation());
            StartCoroutine(TsShoot());

            // Iniciar cooldown
            if (thunderCoroutine != null)
            {
                StopCoroutine(thunderCoroutine);
            }
            thunderCoroutine = StartCoroutine(CardDeactivated2());
        }
    }



    private IEnumerator TsShoot()
    {
        yield return new WaitForSeconds(cooldown2);
        canThunder = true;
    }
    public IEnumerator ThunderAnimation()
    {
        
        GameObject th = Instantiate(tsPrefab, throwPoint.position, Quaternion.identity);

        rb.AddForce(new Vector2(0, tsScript.thunderForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(tsScript.timeForNextTwinkle);

        Destroy(th);

      

    }

   
       private IEnumerator CardDeactivated2()
    {
      
        float cooldown = 8f;
        SpriteRenderer cardRenderer = tsCard.GetComponent<SpriteRenderer>();

        canThunder = false;
        tsCard.SetActive(false);
        cardRenderer.color = Color.blue;
        


        yield return new WaitForSeconds(cooldown);
        

        // Al terminar el cooldown, restaurar estado

        canThunder = true;
        tsCard.SetActive(true);


        cardRenderer.color = Color.white;
    }


  


}
