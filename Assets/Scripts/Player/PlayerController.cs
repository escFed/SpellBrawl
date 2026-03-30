<<<<<<< Updated upstream
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
=======
﻿using UnityEngine;
using UnityEngine.InputSystem;
>>>>>>> Stashed changes

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

    [Header("Cards")]
    [SerializeField] private GameObject slotCard;
    [SerializeField] private GameObject slotCard1;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;


<<<<<<< Updated upstream
=======
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public JabState JabState { get; private set; }
    public ForwardTiltState ForwardTiltState { get; private set; }
    public UpTiltState UpTiltState { get; private set; }
    public CardState CardState { get; private set; }
    public DieState DieState { get; private set; }
>>>>>>> Stashed changes

    [Header("AI")]
    [SerializeField] private GameObject e;

    [Header("Input Responsible")]
    [SerializeField] GameObject hitBox;
    [SerializeField] Coroutine coroutine;

    public Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isGrounded;

<<<<<<< Updated upstream
=======
    public float stunTimer;
    public Transform throwPoint;
>>>>>>> Stashed changes

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

<<<<<<< Updated upstream
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
      
        
=======
    private void Start()
    {
        stateMachine.Initialize(IdleState);

        PlayerInput playerInput = GetComponent<PlayerInput>();
        int playerIndex = playerInput != null ? playerInput.playerIndex : 0;

        FireBallCard fireCard = slotCard.GetComponent<FireBallCard>();
        ThunderStrikeCard thunderCard = slotCard1.GetComponent<ThunderStrikeCard>();

        if (CardUIManager.Instance != null)
        {
            if (playerIndex == 0)
            {
                if (fireCard != null) fireCard.SetUI(CardUIManager.Instance.p1_fireCard);
                if (thunderCard != null) thunderCard.SetUI(CardUIManager.Instance.p1_thunderCard);
            }
            else if (playerIndex == 1)
            {
                if (fireCard != null) fireCard.SetUI(CardUIManager.Instance.p2_fireCard);
                if (thunderCard != null) thunderCard.SetUI(CardUIManager.Instance.p2_thunderCard);
            }
        }
>>>>>>> Stashed changes
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

<<<<<<< Updated upstream
        
       

   
         
=======
        if (inputManager.HasBufferedFire)
        {
            TryUseCard(1);
            inputManager.ConsumeFire();
        }

        if (inputManager.HasBufferedThunder)
        {
            TryUseCard(2);
            inputManager.ConsumeThunder();
        }

        stateMachine.Update();
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    private void OnDrawGizmosSelected()
=======
    public void TryUseCard(int slotIndex)
    {

        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState)
            return;

        ICardable cardToUse = null;

        if (slotIndex == 1 && slotCard != null)
            cardToUse = slotCard.GetComponent<ICardable>();
        else if (slotIndex == 2 && slotCard1 != null)
            cardToUse = slotCard1.GetComponent<ICardable>();

        if (cardToUse != null)
        {
            CardState.SetCard(cardToUse, 0.5f);
            stateMachine.ChangeState(CardState);
        }
    }

    public IState ResolveAttackState()
>>>>>>> Stashed changes
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
          

                if(hit.collider != null && hit.collider.CompareTag("AICharacter")) 
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
<<<<<<< Updated upstream
    private IEnumerator FbShoot()
    {
        yield return new WaitForSeconds(cooldown);
        canFbShot = true;
    }


    private void OnThunder(InputAction.CallbackContext ctx)
    {
      

        
        if (tsCard.activeSelf)
        {
            StartCoroutine(ThunderAnimation());
        }

        if(thunderCoroutine  != null)
        {
            StopCoroutine(thunderCoroutine);
        }

        currentThunderTime += Time.deltaTime;

        if (currentThunderTime >= 1f)
            
        {
            canThunder = false;
            tsCard.SetActive(false);
        }
        thunderCoroutine = StartCoroutine(CardDeactivated2());

        StartCoroutine(TsShoot());

        
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
        float time = 0f;
        float duration = 6f;

        Vector3 startPos = tsCard.transform.position;
        Vector3 targetPos = startPos + new Vector3(-1f, 3f, 0f);


        while (time < duration)

        {
            time += Time.deltaTime;
            tsCard.transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            yield return null;

        }

        tsCard.SetActive(false);
    }


  


}
=======

}
>>>>>>> Stashed changes
