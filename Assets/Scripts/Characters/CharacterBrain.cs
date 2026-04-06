using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBrain : MonoBehaviour ,IInputProvider
{
    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float specialBufferTime = 0.15f;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedJump => jumpTimer > 0;
    public bool HasBufferedAttack => attackTimer > 0;
    public bool HasBufferedSpecial => specialTimer > 0;

    private float attackTimer;
    private float jumpTimer;
    private float specialTimer;

    private void Update()
    {
        if (jumpTimer > 0) jumpTimer -= Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (specialTimer > 0) specialTimer -= Time.deltaTime;
    }

    public void OnMove(InputValue value) => CurrentDirection = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed) jumpTimer = jumpBufferTime; }
    public void OnAttack(InputValue value) { if (value.isPressed) attackTimer = attackBufferTime; }
    public void OnSpecial(InputValue value) { if (value.isPressed) specialTimer = specialBufferTime; }

    public void ConsumeJump() => jumpTimer = 0;
    public void ConsumeAttack() => attackTimer = 0;
    public void ConsumeSpecial() => specialTimer = 0;

    public void ClearAllInputs()
    {
        CurrentDirection = Vector2.zero;
        ConsumeJump();
        ConsumeAttack();
        ConsumeSpecial();
    }
}