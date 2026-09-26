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
        if (SelectionManager.Instance == null)
        {
            Debug.LogWarning("�Inicia desde el MainMenu para que el SelectionManager exista!");
            return;
        }

        SelectionManager selection = SelectionManager.Instance;
        if (selection.characterDb == null || p1SpawnPoint == null || p2SpawnPoint == null)
        {
            Debug.LogError("[RespawnManager] CharacterDatabase and both spawn points are required.", this);
            return;
        }

        if (!IsValidCharacterIndex(selection.characterDb, selection.p1SelectedIndex) ||
            !IsValidCharacterIndex(selection.characterDb, selection.p2SelectedIndex))
        {
            Debug.LogError("[RespawnManager] Both players need a valid character selection.", this);
            return;
        }

        p1Mode = selection.GetControlMode(PlayerSlot.PlayerOne);
        p2Mode = selection.GetControlMode(PlayerSlot.PlayerTwo);

        CharacterStats p1Stats = selection.characterDb.GetCharacter(selection.p1SelectedIndex);
        p1Instance = Instantiate(p1Stats.characterPrefab, p1SpawnPoint.position, Quaternion.identity);

        CharacterCoordinator p1Controller = null;

        if (p1Instance.TryGetComponent(out CharacterCoordinator p1Ctrl))
        {
            p1Controller = p1Ctrl;
            p1Ctrl.ConfigureControl(PlayerSlot.PlayerOne, p1Mode);
        }

        Vector3 p1Scale = p1Instance.transform.localScale;
        p1Scale.x = Mathf.Abs(p1Scale.x);
        p1Instance.transform.localScale = p1Scale;

        CharacterStats p2Stats = selection.characterDb.GetCharacter(selection.p2SelectedIndex);
        p2Instance = Instantiate(p2Stats.characterPrefab, p2SpawnPoint.position, Quaternion.identity);

        CharacterCoordinator p2Controller = null;

        if (p2Instance.TryGetComponent(out CharacterCoordinator p2Ctrl))
        {
            p2Controller = p2Ctrl;
            p2Ctrl.ConfigureControl(PlayerSlot.PlayerTwo, p2Mode);
        }

        Vector3 p2Scale = p2Instance.transform.localScale;
        p2Scale.x = -Mathf.Abs(p2Scale.x);
        p2Instance.transform.localScale = p2Scale;

        if (selection.matchMode == MatchMode.PlayerVsPlayer)
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
