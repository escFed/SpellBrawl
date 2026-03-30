using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Color Palettes")]
    [SerializeField] private Color p1Color = Color.green;
    [SerializeField] private Color p2Color = Color.red;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput != null)
        {
            ApplyColor(playerInput.playerIndex);
        }
    }

    private void ApplyColor(int playerIndex)
    {
        if (playerIndex == 0)
        {
            spriteRenderer.color = p1Color;
        }
        else if (playerIndex == 1)
        {
            spriteRenderer.color = p2Color;
        }
    }
}
