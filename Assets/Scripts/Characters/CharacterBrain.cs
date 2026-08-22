using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBrain : MonoBehaviour, IInputProvider
{
    private const string ShieldActionName = "Shield";
    private const string HeavyAttackActionName = "HeavyAttack";

    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime = 0.15f;
    [SerializeField] private float grabBufferTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float cardBufferTime = 0.15f;
    [SerializeField] private float evadeBufferTime = 0.15f;
    [SerializeField] private float dashBufferTime = 0.15f;
    [SerializeField] private float heavyAttackBufferTime = 0.15f;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump => jumpTimer > 0;
    public bool HasBufferedAttack => attackTimer > 0;
    public bool HasBufferedGrab => grabTimer > 0;
    public bool HasBufferedHand1 => hand1Timer > 0;
    public bool HasBufferedHand2 => hand2Timer > 0;
    public bool HasBufferedHand3 => hand3Timer > 0;
    public bool HasBufferedHand4 => hand4Timer > 0;
    public bool HasBufferedParry => parryTimer > 0;
    public bool HasBufferedShield => shieldTimer > 0;
    public bool HasBufferedEvade => evadeTimer > 0;
    public bool HasBufferedDash => dashTimer > 0;
    public bool IsShieldHeld { get; private set; }
    public bool HasBufferedDrawCards => drawCardsTimer > 0;
    public bool HasBufferedHeavyAttack => heavyAttackTimer > 0f;
    public bool IsHeavyAttackHeld { get; private set; }
    public bool WasHeavyAttackReleased => heavyAttackReleaseTimer > 0f;

    private float attackTimer, grabTimer, jumpTimer;
    private float hand1Timer, hand2Timer, hand3Timer, hand4Timer;
    private float drawCardsTimer;
    private float parryTimer;
    private float shieldTimer;
    private float evadeTimer;
    private float dashTimer;
    private float heavyAttackTimer;
    private float heavyAttackReleaseTimer;
    private PlayerInput playerInput;
    private InputAction shieldAction;
    private InputAction heavyAttackAction;
    private bool heavyAttackRequiresRelease;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        CacheInputActions();
    }

    private void OnEnable()
    {
        CacheInputActions();

        bool isHeavyAttackPressed = heavyAttackAction != null && heavyAttackAction.IsPressed();
        IsHeavyAttackHeld = isHeavyAttackPressed;
        heavyAttackRequiresRelease = isHeavyAttackPressed;
    }

    private void OnDisable()
    {
        IsShieldHeld = false;
        IsHeavyAttackHeld = false;
        ConsumeParry();
        ConsumeShield();
        ConsumeHeavyAttack();
        ConsumeHeavyAttackRelease();
    }

    private void Update()
    {
        if (shieldAction != null)
            IsShieldHeld = shieldAction.IsPressed();

        if (heavyAttackAction != null)
            UpdateHeavyAttackState(heavyAttackAction.IsPressed());

        if (jumpTimer > 0) jumpTimer -= Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (grabTimer > 0) grabTimer -= Time.deltaTime;
        if (hand1Timer > 0) hand1Timer -= Time.deltaTime;
        if (hand2Timer > 0) hand2Timer -= Time.deltaTime;
        if (hand3Timer > 0) hand3Timer -= Time.deltaTime;
        if (hand4Timer > 0) hand4Timer -= Time.deltaTime;
        if (drawCardsTimer > 0) drawCardsTimer -= Time.deltaTime;
        if (parryTimer > 0) parryTimer -= Time.deltaTime;
        if (shieldTimer > 0) shieldTimer -= Time.deltaTime;
        if (evadeTimer > 0) evadeTimer -= Time.deltaTime;
        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        if (heavyAttackTimer > 0) heavyAttackTimer -= Time.deltaTime;
        if (heavyAttackReleaseTimer > 0) heavyAttackReleaseTimer -= Time.deltaTime;
    }

    public void OnMove(InputValue value) => CurrentDirection = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed) jumpTimer = jumpBufferTime; }
    public void OnAttack(InputValue value) { if (value.isPressed) attackTimer = attackBufferTime; }
    public void OnGrab(InputValue value) { if (value.isPressed) grabTimer = grabBufferTime; }
    public void OnDrawCards(InputValue value) { if (value.isPressed) drawCardsTimer = cardBufferTime; }
    public void OnHand1(InputValue value) { if (value.isPressed) hand1Timer = cardBufferTime; }
    public void OnHand2(InputValue value) { if (value.isPressed) hand2Timer = cardBufferTime; }
    public void OnHand3(InputValue value) { if (value.isPressed) hand3Timer = cardBufferTime; }
    public void OnHand4(InputValue value) { if (value.isPressed) hand4Timer = cardBufferTime; }
    public void OnParry(InputValue value) { if (value.isPressed) parryTimer = cardBufferTime; }
    public void OnEvade(InputValue value) => BufferEvade(value);
    public void OnDash(InputValue value) { if (value.isPressed) dashTimer = dashBufferTime; }
    public void OnHeavyAttack(InputValue value)
    {
        UpdateHeavyAttackState(value.isPressed);
    }

    // Compatibility with the current Input Actions asset while it still exposes
    // separate Roll and Dodge actions bound to the same button.
    public void OnRoll(InputValue value) => BufferEvade(value);
    public void OnDodge(InputValue value) => BufferEvade(value);
    public void OnShield(InputValue value)
    {
        IsShieldHeld = value.isPressed;

        if (value.isPressed)
            shieldTimer = cardBufferTime;
    }

    public void ConsumeJump() => jumpTimer = 0;
    public void ConsumeAttack() => attackTimer = 0;
    public void ConsumeGrab() => grabTimer = 0;
    public void ConsumeHand1() => hand1Timer = 0;
    public void ConsumeHand2() => hand2Timer = 0;
    public void ConsumeHand3() => hand3Timer = 0;
    public void ConsumeHand4() => hand4Timer = 0;
    public void ConsumeParry() => parryTimer = 0;
    public void ConsumeShield() => shieldTimer = 0;
    public void ConsumeEvade() => evadeTimer = 0;
    public void ConsumeDash() => dashTimer = 0;
    public void ConsumeHeavyAttack() => heavyAttackTimer = 0f;
    public void ConsumeHeavyAttackRelease() => heavyAttackReleaseTimer = 0f;
    public void ConsumeDrawCards() => drawCardsTimer = 0;

    public void ClearAllInputs()
    {
        heavyAttackRequiresRelease = IsHeavyAttackHeld ||
            (heavyAttackAction != null && heavyAttackAction.IsPressed());

        CurrentDirection = Vector2.zero;
        ConsumeJump();
        ConsumeAttack();
        ConsumeGrab();
        ConsumeHand1();
        ConsumeHand2();
        ConsumeHand3();
        ConsumeHand4();
        ConsumeParry();
        ConsumeShield();
        ConsumeEvade();
        ConsumeDash();
        ConsumeHeavyAttack();
        ConsumeHeavyAttackRelease();
        IsShieldHeld = false;
        IsHeavyAttackHeld = false;
        ConsumeDrawCards();
    }

    private void BufferEvade(InputValue value)
    {
        if (value.isPressed)
            evadeTimer = evadeBufferTime;
    }

    private void UpdateHeavyAttackState(bool isPressed)
    {
        if (heavyAttackRequiresRelease)
        {
            IsHeavyAttackHeld = isPressed;

            if (!isPressed)
            {
                heavyAttackRequiresRelease = false;
                ConsumeHeavyAttackRelease();
            }

            return;
        }

        bool wasHeld = IsHeavyAttackHeld;
        IsHeavyAttackHeld = isPressed;

        if (isPressed && !wasHeld)
        {
            heavyAttackTimer = heavyAttackBufferTime;
            heavyAttackReleaseTimer = 0f;
        }
        else if (!isPressed && wasHeld)
        {
            heavyAttackReleaseTimer = heavyAttackBufferTime;
        }
    }

    private void CacheInputActions()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        shieldAction = playerInput != null
            ? playerInput.actions?.FindAction(ShieldActionName, false)
            : null;

        heavyAttackAction = playerInput != null
            ? playerInput.actions?.FindAction(HeavyAttackActionName, false)
            : null;
    }
}
