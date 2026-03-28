using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class FightingInputManager : MonoBehaviour
{
    [Header("Input Buffer Settings")]
    [SerializeField] private float attackBufferTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    public Vector2 CurrentDirection { get; private set; }
    public bool HasBufferedAttack { get; private set; }
    public bool HasBufferedJump { get; private set; }

    private float attackTimer;
    private float jumpTimer;

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            HasBufferedAttack = true;
        }
        else HasBufferedAttack = false;

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            HasBufferedJump = true;
        }
        else HasBufferedJump = false;
    }

    public void OnMove(InputValue value)
    {
        CurrentDirection = value.Get<Vector2>();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) attackTimer = attackBufferTime;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) jumpTimer = jumpBufferTime;
    }

    public void ConsumeAttack() { attackTimer = 0f; HasBufferedAttack = false; }
    public void ConsumeJump() { jumpTimer = 0f; HasBufferedJump = false; }

    public void ClearAllInputs()
    {
        ConsumeAttack();
        ConsumeJump();
        CurrentDirection = Vector2.zero;
    }
}
