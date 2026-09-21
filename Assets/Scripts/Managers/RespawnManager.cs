using UnityEngine;

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

        CharacterStats p1Stats = SelectionManager.Instance.characterDb.GetCharacter(SelectionManager.Instance.p1SelectedIndex);
        p1Instance = Instantiate(p1Stats.characterPrefab, p1SpawnPoint.position, Quaternion.identity);

        if (p1Instance.TryGetComponent(out CharacterCoordinator p1Ctrl))
        {
            p1Ctrl.ConfigureControl(PlayerSlot.PlayerOne, p1Mode);
            p1Ctrl.SetControlsEnabled(false);
        }

        Vector3 p1Scale = p1Instance.transform.localScale;
        p1Scale.x = Mathf.Abs(p1Scale.x);
        p1Instance.transform.localScale = p1Scale;

        CharacterStats aiStats = SelectionManager.Instance.characterDb.GetCharacter(SelectionManager.Instance.aiSelectedIndex);
        p2Instance = Instantiate(aiStats.characterPrefab, p2SpawnPoint.position, Quaternion.identity);

        if (p2Instance.TryGetComponent(out CharacterCoordinator p2Ctrl))
        {
            p2Ctrl.ConfigureControl(PlayerSlot.PlayerTwo, p2Mode);
            p2Ctrl.SetControlsEnabled(false);
        }

        Vector3 aiScale = p2Instance.transform.localScale;
        aiScale.x = -Mathf.Abs(aiScale.x);
        p2Instance.transform.localScale = aiScale;
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
