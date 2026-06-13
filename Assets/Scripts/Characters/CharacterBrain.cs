using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBrain : MonoBehaviour, IInputProvider
{
    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime  = 0.15f;
    [SerializeField] private float jumpBufferTime    = 0.15f;
    [SerializeField] private float cardBufferTime    = 0.15f;
    [Header("Shield / Parry Threshold")]
    [Tooltip("Hold J shorter than this = Parry tap. Longer = Shield.")]
    [SerializeField] private float shieldHoldThreshold = 0.15f;

    
    private InputAction parryAction;

    
    private bool  parryPrevPressed;  
    private float parryHoldTimer;    

    
    private float attackTimer, jumpTimer;
    private float hand1Timer, hand2Timer, hand3Timer, hand4Timer;
    private float drawCardsTimer;
    private float parryTimer;        

   
    public Vector2 CurrentDirection  { get; private set; }
    public bool HasBufferedJump      => jumpTimer      > 0;
    public bool HasBufferedAttack    => attackTimer    > 0;
    public bool HasBufferedHand1     => hand1Timer     > 0;
    public bool HasBufferedHand2     => hand2Timer     > 0;
    public bool HasBufferedHand3     => hand3Timer     > 0;
    public bool HasBufferedHand4     => hand4Timer     > 0;
    public bool HasBufferedParry     => parryTimer     > 0;
    public bool HasBufferedDrawCards => drawCardsTimer > 0;

    
    public bool IsShieldHeld =>
        parryAction != null &&
        parryAction.IsPressed() &&
        parryHoldTimer >= shieldHoldThreshold;

    

    private void Awake()
    {   
        var pi = GetComponent<PlayerInput>();
        if (pi != null) parryAction = pi.actions["Parry"];
    }

    private void Update()
    {
       
        bool parryNowPressed = parryAction != null && parryAction.IsPressed();

        if (parryNowPressed && !parryPrevPressed)
        {
            
            parryHoldTimer = 0f;
        }

        if (parryNowPressed)
            parryHoldTimer += Time.deltaTime;

        if (!parryNowPressed && parryPrevPressed)
        {
            
            if (parryHoldTimer < shieldHoldThreshold)
                parryTimer = cardBufferTime;

            parryHoldTimer = 0f;
        }

        parryPrevPressed = parryNowPressed;

       
        if (jumpTimer      > 0) jumpTimer      -= Time.deltaTime;
        if (attackTimer    > 0) attackTimer    -= Time.deltaTime;
        if (hand1Timer     > 0) hand1Timer     -= Time.deltaTime;
        if (hand2Timer     > 0) hand2Timer     -= Time.deltaTime;
        if (hand3Timer     > 0) hand3Timer     -= Time.deltaTime;
        if (hand4Timer     > 0) hand4Timer     -= Time.deltaTime;
        if (drawCardsTimer > 0) drawCardsTimer -= Time.deltaTime;
        if (parryTimer     > 0) parryTimer     -= Time.deltaTime;
    }

   
    public void OnMove(InputValue value)      => CurrentDirection = value.Get<Vector2>();
    public void OnJump(InputValue value)      { if (value.isPressed) jumpTimer   = jumpBufferTime; }
    public void OnAttack(InputValue value)    { if (value.isPressed) attackTimer = attackBufferTime; }
    public void OnDrawCards(InputValue value) { if (value.isPressed) drawCardsTimer = cardBufferTime; }
    public void OnHand1(InputValue value)     { if (value.isPressed) hand1Timer  = cardBufferTime; }
    public void OnHand2(InputValue value)     { if (value.isPressed) hand2Timer  = cardBufferTime; }
    public void OnHand3(InputValue value)     { if (value.isPressed) hand3Timer  = cardBufferTime; }
    public void OnHand4(InputValue value)     { if (value.isPressed) hand4Timer  = cardBufferTime; }
   
    public void OnParry(InputValue value)     { }

  
    public void ConsumeJump()      => jumpTimer      = 0;
    public void ConsumeAttack()    => attackTimer    = 0;
    public void ConsumeHand1()     => hand1Timer     = 0;
    public void ConsumeHand2()     => hand2Timer     = 0;
    public void ConsumeHand3()     => hand3Timer     = 0;
    public void ConsumeHand4()     => hand4Timer     = 0;
    public void ConsumeParry()     => parryTimer     = 0;
    public void ConsumeDrawCards() => drawCardsTimer = 0;

    public void ClearAllInputs()
    {
        CurrentDirection = Vector2.zero;
        ConsumeJump(); ConsumeAttack();
        ConsumeHand1(); ConsumeHand2(); ConsumeHand3(); ConsumeHand4();
        ConsumeParry(); ConsumeDrawCards();
        parryHoldTimer  = 0f;
        parryPrevPressed = false;
    }
}
