using UnityEngine;
using UnityEngine.InputSystem;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("SpawnPoints")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;
    public Transform stockRespawnPoint;

    [Header("Control Modes")]
    [SerializeField] private PlayerMode p1Mode = PlayerMode.Player;
    [SerializeField] private PlayerMode p2Mode = PlayerMode.AI;

    [Header("Life Respawn")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private float respawnDropWindow = 5f;
    [SerializeField] private float respawnProtectionDuration = 2f;
    [HideInInspector] public GameObject p1Instance;
    [HideInInspector] public GameObject p2Instance;
    private bool refreshLocalControls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitialSpawn();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad &&
            (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected ||
             change == InputDeviceChange.Enabled || change == InputDeviceChange.Removed ||
             change == InputDeviceChange.Disconnected || change == InputDeviceChange.Disabled))
            refreshLocalControls = true;
    }

    private void Update()
    {
        if (!refreshLocalControls)
            return;

        refreshLocalControls = false;
        if (SelectionManager.Instance == null || SelectionManager.Instance.matchMode != MatchMode.PlayerVsPlayer ||
            p1Instance == null || p2Instance == null)
            return;

        ConfigureLocal(p1Instance.GetComponent<CharacterCoordinator>(), p2Instance.GetComponent<CharacterCoordinator>(), true);
    }

    private void InitialSpawn()
    {
        if (!ValidateSetup(out SelectionManager selection))
            return;

        p1Mode = selection.GetControlMode(PlayerSlot.PlayerOne);
        p2Mode = selection.GetControlMode(PlayerSlot.PlayerTwo);

        CharacterStats p1Stats = selection.characterDb.GetCharacter(selection.p1SelectedIndex);
        CharacterStats p2Stats = selection.characterDb.GetCharacter(selection.p2SelectedIndex);
        CharacterCoordinator p1Controller = SpawnCharacter(
            p1Stats,
            p1SpawnPoint,
            PlayerSlot.PlayerOne,
            p1Mode,
            out p1Instance);
        CharacterCoordinator p2Controller = SpawnCharacter(
            p2Stats,
            p2SpawnPoint,
            PlayerSlot.PlayerTwo,
            p2Mode,
            out p2Instance);

        if (p1Stats == p2Stats && p2Instance != null &&
            p2Instance.TryGetComponent(out AmiroBlueVariant blueVariant))
            blueVariant.Apply();

        ConfigureInitialControls(selection.matchMode, p1Controller, p2Controller);

        MatchCameraDirector cameraDirector = FindAnyObjectByType<MatchCameraDirector>();
        cameraDirector?.RefreshTargets();
    }

    private bool ValidateSetup(out SelectionManager selection)
    {
        selection = SelectionManager.Instance;
        if (selection == null)
        {
            Debug.LogWarning("[RespawnManager] Start from MainMenu so SelectionManager exists.", this);
            return false;
        }

        if (selection.characterDb == null || p1SpawnPoint == null || p2SpawnPoint == null)
        {
            Debug.LogError("[RespawnManager] CharacterDatabase and both spawn points are required.", this);
            return false;
        }

        if (!IsValidCharacterIndex(selection.characterDb, selection.p1SelectedIndex) ||
            !IsValidCharacterIndex(selection.characterDb, selection.p2SelectedIndex))
        {
            Debug.LogError("[RespawnManager] Both players need a valid character selection.", this);
            return false;
        }

        return true;
    }

    private CharacterCoordinator SpawnCharacter(
        CharacterStats stats,
        Transform spawnPoint,
        PlayerSlot slot,
        PlayerMode mode,
        out GameObject instance)
    {
        if (stats == null || stats.characterPrefab == null)
        {
            Debug.LogError($"[RespawnManager] {slot} needs a valid character prefab.", this);
            instance = null;
            return null;
        }

        instance = Instantiate(stats.characterPrefab, spawnPoint.position, Quaternion.identity);

        CharacterCoordinator controller = null;
        if (instance.TryGetComponent(out CharacterCoordinator configuredController))
        {
            controller = configuredController;
            controller.ConfigureControl(slot, mode);
        }

        controller?.Combat.FaceDirection(slot == PlayerSlot.PlayerOne ? 1f : -1f);
        if (controller != null && GameManager.Instance != null)
            GameManager.Instance.Statistics.StartLife(controller.PlayerIndex, Time.time);

        return controller;
    }

    private void ConfigureInitialControls(
        MatchMode matchMode,
        CharacterCoordinator p1Controller,
        CharacterCoordinator p2Controller)
    {
        if (matchMode == MatchMode.PlayerVsPlayer)
            ConfigureLocal(p1Controller, p2Controller);

        p1Controller?.SetControlsEnabled(false);
        p2Controller?.SetControlsEnabled(false);
    }

    private static bool IsValidCharacterIndex(CharacterDatabase database, int index)
    {
        return database != null && index >= 0 && index < database.CharacterCount;
    }

    private void ConfigureLocal(
        CharacterCoordinator p1Controller,
        CharacterCoordinator p2Controller,
        bool preserveExistingGamepads = false)
    {
        if (p1Controller == null || p2Controller == null)
            return;

        PlayerInput p1Input = p1Controller.GetComponent<PlayerInput>();
        PlayerInput p2Input = p2Controller.GetComponent<PlayerInput>();
        if (p1Input == null || p2Input == null)
        {
            Debug.LogError("[RespawnManager] Both human characters require PlayerInput components.", this);
            return;
        }

        p1Input.neverAutoSwitchControlSchemes = true;
        p2Input.neverAutoSwitchControlSchemes = true;

        if (Gamepad.all.Count >= 2)
        {
            Gamepad p1Pad = preserveExistingGamepads ? GetPairedGamepad(p1Input) : null;
            Gamepad p2Pad = preserveExistingGamepads ? GetPairedGamepad(p2Input) : null;
            if (p1Pad != null && p2Pad != null && p1Pad != p2Pad)
                return;

            if (p1Pad != null)
                p2Input.SwitchCurrentControlScheme("Gamepad", FirstOtherGamepad(p1Pad));
            else if (p2Pad != null)
                p1Input.SwitchCurrentControlScheme("Gamepad", FirstOtherGamepad(p2Pad));
            else
            {
                p1Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
                p2Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
            }
            return;
        }

        if (Gamepad.all.Count >= 1 && Keyboard.current != null && Mouse.current != null)
        {
            if (preserveExistingGamepads && GetPairedGamepad(p1Input) == Gamepad.all[0])
            {
                p2Input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            }
            else
            {
                p1Input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
                p2Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            }
            return;
        }

        Debug.LogError(
            "[RespawnManager] Local 1 vs 1 requires two gamepads, or a keyboard/mouse plus one gamepad. " +
            "The current input bindings cannot split one keyboard between both players.",
            this);
    }

    private static Gamepad GetPairedGamepad(PlayerInput input)
    {
        foreach (InputDevice device in input.devices)
        {
            if (device is not Gamepad pad)
                continue;

            foreach (Gamepad connected in Gamepad.all)
                if (connected == pad)
                    return pad;
        }

        return null;
    }

    private static Gamepad FirstOtherGamepad(Gamepad pairedPad)
    {
        foreach (Gamepad pad in Gamepad.all)
            if (pad != pairedPad)
                return pad;

        return null;
    }

    public void RespawnPlayerAfterFall(CharacterHealth health, int playerIndex)
    {
        if (health == null || health.gameObject == null) return;

        Transform targetSpawn = stockRespawnPoint != null
            ? stockRespawnPoint
            : playerIndex == 0 ? p1SpawnPoint : p2SpawnPoint;

        if (targetSpawn == null) return;

        health.BeginRespawnSequence(targetSpawn.position, respawnDelay, respawnProtectionDuration, respawnDropWindow);
    }

    public void ResetRoundPositionsAndHealth()
    {
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator p in allPlayers)
        {
            CharacterHealth pHealth = p.GetComponent<CharacterHealth>();

            if (p.gameObject == p1Instance)
            {
                if (p1SpawnPoint != null)
                {
                    p.transform.position = p1SpawnPoint.position;
                    p.Combat.UnlockFacing();
                    p.Combat.FaceDirection(1f);
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
            else if (p.gameObject == p2Instance)
            {
                if (p2SpawnPoint != null)
                {
                    p.transform.position = p2SpawnPoint.position;
                    p.Combat.UnlockFacing();
                    p.Combat.FaceDirection(-1f);
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
        }
    }
}
