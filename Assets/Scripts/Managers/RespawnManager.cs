using UnityEngine;
using UnityEngine.InputSystem;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("SpawnPoints")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;

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

        if (p1Instance.TryGetComponent(out PlayerController p1Ctrl)) p1Ctrl.PlayerIndex = 0;

        if (p1Instance.TryGetComponent(out CharacterAI p1AI)) p1AI.enabled = false;
        if (p1Instance.TryGetComponent(out PlayerInput p1Input)) p1Input.enabled = true;
        if (p1Instance.TryGetComponent(out CharacterBrain p1Brain)) p1Brain.enabled = true;

        Vector3 p1Scale = p1Instance.transform.localScale;
        p1Scale.x = Mathf.Abs(p1Scale.x);
        p1Instance.transform.localScale = p1Scale;

        CharacterStats aiStats = SelectionManager.Instance.characterDb.GetCharacter(SelectionManager.Instance.aiSelectedIndex);
        p2Instance = Instantiate(aiStats.characterPrefab, p2SpawnPoint.position, Quaternion.identity);

        if (p2Instance.TryGetComponent(out PlayerController p2Ctrl)) p2Ctrl.PlayerIndex = 1;

        if (p2Instance.TryGetComponent(out PlayerInput aiInput)) Destroy(aiInput);
        if (p2Instance.TryGetComponent(out CharacterBrain aiBrain)) Destroy(aiBrain);

        if (p2Instance.TryGetComponent(out CharacterAI aiAI)) aiAI.enabled = true;

        Vector3 aiScale = p2Instance.transform.localScale;
        aiScale.x = -Mathf.Abs(aiScale.x);
        p2Instance.transform.localScale = aiScale;
    }

    public void RespawnPlayerAfterFall(CharacterHealth health, int playerIndex)
    {
        Transform targetSpawn = (health.gameObject == p1Instance) ? p1SpawnPoint : p2SpawnPoint;
        health.transform.position = targetSpawn.position;
        Rigidbody2D rb = health.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void ResetRoundPositionsAndHealth()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
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
