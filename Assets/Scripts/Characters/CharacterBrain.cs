using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBrain : MonoBehaviour ,IInputProvider
{
    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float cardBufferTime = 0.15f;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedJump { get; private set; }

    public bool HasBufferedFire { get; private set; }
    public bool HasBufferedThunder { get; private set; }

    private float attackTimer;
    private float jumpTimer;
    private float fireTimer;    
    private float thunderTimer; 

    private void Update()
    {

        if (attackTimer > 0) { attackTimer -= Time.deltaTime; HasBufferedAttack = true; }
        else HasBufferedAttack = false;

        if (jumpTimer > 0) { jumpTimer -= Time.deltaTime; HasBufferedJump = true; }
        else HasBufferedJump = false;

        if (fireTimer > 0) { fireTimer -= Time.deltaTime; HasBufferedFire = true; }
        else HasBufferedFire = false;

        if (thunderTimer > 0) { thunderTimer -= Time.deltaTime; HasBufferedThunder = true; }
        else HasBufferedThunder = false;
    }

    public void OnMove(InputValue value) => CurrentDirection = value.Get<Vector2>();
    public void OnAttack(InputValue value) { if (value.isPressed) attackTimer = attackBufferTime; }
    public void OnJump(InputValue value) { if (value.isPressed) jumpTimer = jumpBufferTime; }
    public void OnFire(InputValue value) { if (value.isPressed) fireTimer = cardBufferTime; }
    public void OnThunder(InputValue value) { if (value.isPressed) thunderTimer = cardBufferTime; }

    public void ConsumeAttack() { attackTimer = 0f; HasBufferedAttack = false; }
    public void ConsumeJump() { jumpTimer = 0f; HasBufferedJump = false; }

    public void ConsumeFire() { fireTimer = 0f; HasBufferedFire = false; }
    public void ConsumeThunder() { thunderTimer = 0f; HasBufferedThunder = false; }

    public void ClearAllInputs()
    {
        ConsumeAttack();
        ConsumeJump();
        ConsumeFire();
        ConsumeThunder();
        CurrentDirection = Vector2.zero;
    }
}