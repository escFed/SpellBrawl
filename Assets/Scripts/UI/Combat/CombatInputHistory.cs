using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[Serializable]
public struct StickSprites
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;
    public Sprite upLeft;
    public Sprite upRight;
    public Sprite downLeft;
    public Sprite downRight;

    public Sprite ForDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return up;
        if (direction == Vector2Int.down) return down;
        if (direction == Vector2Int.left) return left;
        if (direction == Vector2Int.right) return right;
        if (direction == new Vector2Int(-1, 1)) return upLeft;
        if (direction == new Vector2Int(1, 1)) return upRight;
        if (direction == new Vector2Int(-1, -1)) return downLeft;
        if (direction == new Vector2Int(1, -1)) return downRight;
        return null;
    }
}

[Serializable]
public struct SpriteBinding
{
    public string controlPath;
    public Sprite sprite;

    public SpriteBinding(string controlPath)
    {
        this.controlPath = controlPath;
        sprite = null;
    }
}

public class CombatInputHistory : MonoBehaviour
{
    private const int MaxEntries = 9;

    [Header("UI - top to bottom")]
    [SerializeField] private UnityEngine.UI.Image[] slots = new UnityEngine.UI.Image[MaxEntries];

    [Header("Player (leave empty to follow RespawnManager P1)")]
    [SerializeField] private PlayerInput playerInputOverride;

    [Header("Button and keyboard sprites")]
    // These paths match the current Player action map. Assign the sprites in the Inspector.
    [SerializeField] private SpriteBinding[] spriteBindings =
    {
        new SpriteBinding("<Keyboard>/w"),
        new SpriteBinding("<Keyboard>/a"),
        new SpriteBinding("<Keyboard>/s"),
        new SpriteBinding("<Keyboard>/d"),
        new SpriteBinding("<Keyboard>/upArrow"),
        new SpriteBinding("<Keyboard>/downArrow"),
        new SpriteBinding("<Keyboard>/leftArrow"),
        new SpriteBinding("<Keyboard>/rightArrow"),
        new SpriteBinding("<Keyboard>/j"),
        new SpriteBinding("<Keyboard>/space"),
        new SpriteBinding("<Keyboard>/1"),
        new SpriteBinding("<Keyboard>/2"),
        new SpriteBinding("<Keyboard>/3"),
        new SpriteBinding("<Keyboard>/4"),
        new SpriteBinding("<Keyboard>/r"),
        new SpriteBinding("<Keyboard>/l"),
        new SpriteBinding("<Keyboard>/u"),
        new SpriteBinding("<Keyboard>/i"),
        new SpriteBinding("<Keyboard>/o"),
        new SpriteBinding("<Keyboard>/h"),
        new SpriteBinding("<Gamepad>/buttonWest"),
        new SpriteBinding("<Gamepad>/buttonSouth"),
        new SpriteBinding("<Gamepad>/buttonEast"),
        new SpriteBinding("<Gamepad>/buttonNorth"),
        new SpriteBinding("<Gamepad>/leftShoulder"),
        new SpriteBinding("<Gamepad>/leftTrigger"),
        new SpriteBinding("<Gamepad>/rightShoulder"),
        new SpriteBinding("<Gamepad>/rightTrigger"),
        new SpriteBinding("<Gamepad>/rightStick/up"),
        new SpriteBinding("<Gamepad>/rightStick/down"),
        new SpriteBinding("<Gamepad>/rightStick/left"),
        new SpriteBinding("<Gamepad>/rightStick/right"),
        new SpriteBinding("<Gamepad>/rightStickPress")
    };

    [Header("Gamepad left stick movement")]
    [SerializeField] private StickSprites gamepadMoveSprites;
    [SerializeField, Range(0.1f, 0.9f)] private float moveThreshold = 0.4f;

    private readonly List<Sprite> history = new List<Sprite>(MaxEntries);
    private GameObject cachedPlayerObject;
    private PlayerInput cachedPlayerInput;
    private PlayerInput subscribedPlayerInput;
    private InputActionMap subscribedActionMap;
    private InputAction moveAction;
    private InputControl lastRecordedControl;
    private Vector2Int lastStickDirection;
    private int lastRecordedFrame = -1;

    private void OnEnable()
    {
        ConfigureSlots();
        ClearHistory();
        ConnectToCurrentPlayer();
    }

    private void Update()
    {
        ConnectToCurrentPlayer();
    }

    private void OnDisable()
    {
        DisconnectFromPlayer();
    }

    public void ClearHistory()
    {
        history.Clear();
        lastRecordedControl = null;
        lastRecordedFrame = -1;
        lastStickDirection = Vector2Int.zero;
        RefreshSlots();
    }

    private void ConfigureSlots()
    {
        if (slots == null)
            return;

        foreach (UnityEngine.UI.Image slot in slots)
        {
            if (slot == null)
                continue;

            slot.raycastTarget = false;
            slot.preserveAspect = true;
        }
    }

    private void ConnectToCurrentPlayer()
    {
        PlayerInput candidate = playerInputOverride;

        if (candidate == null)
        {
            GameObject playerObject = RespawnManager.Instance != null ? RespawnManager.Instance.p1Instance : null;
            // ReferenceEquals also notices when Unity has destroyed the previous player object.
            if (!ReferenceEquals(playerObject, cachedPlayerObject))
            {
                cachedPlayerObject = playerObject;
                cachedPlayerInput = playerObject != null ? playerObject.GetComponent<PlayerInput>() : null;
            }

            candidate = cachedPlayerInput;
        }

        if (ReferenceEquals(candidate, subscribedPlayerInput))
            return;

        DisconnectFromPlayer();
        ClearHistory();

        if (candidate == null || candidate.actions == null)
            return;

        InputActionMap actionMap = candidate.actions.FindActionMap("Player", false);
        if (actionMap == null)
            return;

        subscribedPlayerInput = candidate;
        subscribedActionMap = actionMap;

        foreach (InputAction action in actionMap.actions)
        {
            if (action.type == InputActionType.Button)
                action.performed += OnButtonPerformed;
        }

        moveAction = actionMap.FindAction("Move", false);
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }
    }

    private void DisconnectFromPlayer()
    {
        if (subscribedActionMap != null)
        {
            foreach (InputAction action in subscribedActionMap.actions)
            {
                if (action.type == InputActionType.Button)
                    action.performed -= OnButtonPerformed;
            }
        }

        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        moveAction = null;
        subscribedActionMap = null;
        subscribedPlayerInput = null;
    }

    private void OnButtonPerformed(InputAction.CallbackContext context)
    {
        if (PauseMenu.isPaused || context.control == null)
            return;

        AddToHistory(FindSprite(context.control), context.control);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (PauseMenu.isPaused || context.control == null)
            return;

        if (context.control.device is Keyboard)
        {
            // A Value action also performs when a direction key is released.
            if (context.control is ButtonControl key && key.isPressed)
                AddToHistory(FindSprite(context.control), context.control);

            return;
        }

        Vector2Int direction = GetStickDirection(context.ReadValue<Vector2>());

        if (direction == lastStickDirection)
            return;

        lastStickDirection = direction;
        if (direction != Vector2Int.zero)
            AddToHistory(gamepadMoveSprites.ForDirection(direction), context.control);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        lastStickDirection = Vector2Int.zero;
    }

    private Vector2Int GetStickDirection(Vector2 movement)
    {
        int x = Mathf.Abs(movement.x) >= moveThreshold ? (movement.x > 0f ? 1 : -1) : 0;
        int y = Mathf.Abs(movement.y) >= moveThreshold ? (movement.y > 0f ? 1 : -1) : 0;
        return new Vector2Int(x, y);
    }

    private Sprite FindSprite(InputControl control)
    {
        if (spriteBindings == null)
            return null;

        foreach (SpriteBinding binding in spriteBindings)
        {
            if (binding.sprite != null && !string.IsNullOrEmpty(binding.controlPath) &&
                InputControlPath.Matches(binding.controlPath, control))
                return binding.sprite;
        }

        return null;
    }

    private void AddToHistory(Sprite sprite, InputControl control)
    {
        if (sprite == null || slots == null || slots.Length == 0)
            return;

        // Roll and Dodge share a binding; a single physical press gets one icon.
        if (lastRecordedControl == control && lastRecordedFrame == Time.frameCount)
            return;

        lastRecordedControl = control;
        lastRecordedFrame = Time.frameCount;

        if (history.Count == MaxEntries)
            history.RemoveAt(0);

        history.Add(sprite);
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            UnityEngine.UI.Image slot = slots[i];
            if (slot == null)
                continue;

            // Empty slots stay at the top; the newest input occupies the bottom slot.
            int historyIndex = history.Count - slots.Length + i;
            bool visible = historyIndex >= 0;
            slot.sprite = visible ? history[historyIndex] : null;

            // Keep the GameObjects active so a Layout Group preserves all nine positions.
            slot.enabled = visible;
        }
    }
}
