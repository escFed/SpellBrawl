using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBrain : MonoBehaviour, IInputProvider
{
    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float cardBufferTime = 0.15f;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump => jumpTimer > 0;
    public bool HasBufferedAttack => attackTimer > 0;

    public bool HasBufferedHand1 => hand1Timer > 0;
    public bool HasBufferedHand2 => hand2Timer > 0;
    public bool HasBufferedHand3 => hand3Timer > 0;
    public bool HasBufferedHand4 => hand4Timer > 0;
    public bool HasBufferedHand5 => hand5Timer > 0;

    private float attackTimer, jumpTimer;
    private float hand1Timer, hand2Timer, hand3Timer, hand4Timer, hand5Timer;

    private void Update()
    {
        if (jumpTimer > 0) jumpTimer -= Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (hand1Timer > 0) hand1Timer -= Time.deltaTime;
        if (hand2Timer > 0) hand2Timer -= Time.deltaTime;
        if (hand3Timer > 0) hand3Timer -= Time.deltaTime;
        if (hand4Timer > 0) hand4Timer -= Time.deltaTime;
        if (hand5Timer > 0) hand5Timer -= Time.deltaTime;
    }

    public void OnMove(InputValue value) => CurrentDirection = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed) jumpTimer = jumpBufferTime; }
    public void OnAttack(InputValue value) { if (value.isPressed) attackTimer = attackBufferTime; }

    public void OnHand1(InputValue value) { if (value.isPressed) hand1Timer = cardBufferTime; }
    public void OnHand2(InputValue value) { if (value.isPressed) hand2Timer = cardBufferTime; }
    public void OnHand3(InputValue value) { if (value.isPressed) hand3Timer = cardBufferTime; }
    public void OnHand4(InputValue value) { if (value.isPressed) hand4Timer = cardBufferTime; }
    public void OnHand5(InputValue value) { if (value.isPressed) hand5Timer = cardBufferTime; }

    public void ConsumeJump() => jumpTimer = 0;
    public void ConsumeAttack() => attackTimer = 0;
    public void ConsumeHand1() => hand1Timer = 0;
    public void ConsumeHand2() => hand2Timer = 0;
    public void ConsumeHand3() => hand3Timer = 0;
    public void ConsumeHand4() => hand4Timer = 0;
    public void ConsumeHand5() => hand5Timer = 0;

    public void ClearAllInputs()
    {
        CurrentDirection = Vector2.zero;
        ConsumeJump();
        ConsumeAttack();
        ConsumeHand1(); 
        ConsumeHand2(); 
        ConsumeHand3(); 
        ConsumeHand4(); 
        ConsumeHand5();
    }
}