using UnityEngine;
using UnityEngine.InputSystem;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("SpawnPoints")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;

    [Header("Control Modes")]
    [SerializeField] private PlayerMode p1Mode = PlayerMode.Player;
    [SerializeField] private PlayerMode p2Mode = PlayerMode.AI;

    [Header("Life Respawn")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private float respawnProtectionDuration = 2f;
    [HideInInspector] public GameObject p1Instance;
    [HideInInspector] public GameObject p2Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitialSpawn();
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

        ConfigureInitialControls(selection.matchMode, p1Controller, p2Controller);
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

        Vector3 scale = instance.transform.localScale;
        scale.x = slot == PlayerSlot.PlayerOne ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        instance.transform.localScale = scale;

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

    private void ConfigureLocal(CharacterCoordinator p1Controller, CharacterCoordinator p2Controller)
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
            p1Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            p2Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
            return;
        }

        if (Gamepad.all.Count >= 1 && Keyboard.current != null && Mouse.current != null)
        {
            p1Input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            p2Input.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            return;
        }

        Debug.LogError(
            "[RespawnManager] Local 1 vs 1 requires two gamepads, or a keyboard/mouse plus one gamepad. " +
            "The current input bindings cannot split one keyboard between both players.",
            this);
    }

    public void RespawnPlayerAfterFall(CharacterHealth health, int playerIndex)
    {
        if (health == null || health.gameObject == null) return;

        Transform targetSpawn = playerIndex == 0 ? p1SpawnPoint : p2SpawnPoint;

        if (targetSpawn == null) return;

        health.BeginRespawnSequence(targetSpawn.position, respawnDelay, respawnProtectionDuration);
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
                    p.transform.localScale = new Vector3(Mathf.Abs(p.transform.localScale.x), p.transform.localScale.y, p.transform.localScale.z);
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
            else if (p.gameObject == p2Instance)
            {
                if (p2SpawnPoint != null)
                {
                    p.transform.position = p2SpawnPoint.position;
                    p.transform.localScale = new Vector3(-Mathf.Abs(p.transform.localScale.x), p.transform.localScale.y, p.transform.localScale.z);
                    if (pHealth != null) pHealth.ResetHealth();
                }
            }
        }
    }
}
