using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TrainingManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private CharacterDatabase fallbackCharacterDatabase;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Dummy Setup")]
    [SerializeField] private Dummy sceneDummy;
    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private Transform dummySpawnPoint;

    [Header("Optional Debug UI")]
    [SerializeField] private TextMeshProUGUI dummyDamageText;
    [SerializeField] private TextMeshProUGUI playerStateText;
    [SerializeField] private TextMeshProUGUI helpText;

    private PlayerController player;
    private Dummy dummy;
    private Vector3 playerSpawnPosition;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        CharacterDatabase database = GetCharacterDatabase();
        if (database == null || database.CharacterCount == 0)
        {
            Debug.LogError("TrainingRoomManager needs a CharacterDatabase with at least one character.");
            enabled = false;
            return;
        }

        playerSpawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(-3f, 0f, 0f);

        Vector3 dummyPosition = dummySpawnPoint != null ? dummySpawnPoint.position : new Vector3(3f, 0f, 0f);

        SpawnPlayer(database, playerSpawnPosition);
        SetupDummy(dummyPosition);

        if (helpText != null)
            helpText.text = "R: Reset training   |   ESC: Main menu";

        ResetTraining();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
                ResetTraining();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ReturnToMainMenu();
        }

        UpdateDebugUI();
    }

    private void OnDestroy()
    {
        if (dummy != null)
            dummy.DamageChanged -= OnDummyDamageChanged;
    }

    public void ResetTraining()
    {
        player?.Grab?.ReleaseGrabbedTarget();
        dummy?.ResetDummy();

        if (player == null)
            return;

        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
        }

        player.transform.position = playerSpawnPosition;
        player.transform.localScale = new Vector3(Mathf.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);

        player.ActiveInput?.ClearAllInputs();
        player.Health?.ResetHealth();
        player.ChangeState(StateCharacter.Idle);
        player.controlsEnabled = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.isTrainingMode = false;

        SceneManager.LoadScene("MainMenu");
    }

    private CharacterDatabase GetCharacterDatabase()
    {
        if (SelectionManager.Instance != null && SelectionManager.Instance.characterDb != null)
            return SelectionManager.Instance.characterDb;

        return fallbackCharacterDatabase;
    }

    private void SpawnPlayer(CharacterDatabase database, Vector3 position)
    {
        int selectedIndex = SelectionManager.Instance != null ? SelectionManager.Instance.p1SelectedIndex : 0;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, database.CharacterCount - 1);
        CharacterStats stats = database.GetCharacter(selectedIndex);

        if (stats == null || stats.characterPrefab == null)
        {
            Debug.LogError("The selected character has no prefab assigned.");
            return;
        }

        GameObject instance = Instantiate(stats.characterPrefab, position, Quaternion.identity);
        instance.name = stats.characterName + " (Training Player)";
        player = instance.GetComponent<PlayerController>();

        if (player == null)
        {
            Debug.LogError("The selected character prefab needs a PlayerController.");
            return;
        }

        player.PlayerIndex = 0;
        player.cardsEnabled = false;
        player.controlsEnabled = true;

        CharacterAI ai = instance.GetComponent<CharacterAI>();
        if (ai != null) ai.enabled = false;

        CharacterBrain brain = instance.GetComponent<CharacterBrain>();
        if (brain != null) brain.enabled = true;

        PlayerInput playerInput = instance.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;
    }

    private void SetupDummy(Vector3 position)
    {
        dummy = sceneDummy;

        if (dummy == null && dummyPrefab != null)
        {
            GameObject instance = Instantiate(dummyPrefab, position, Quaternion.identity);
            instance.name = dummyPrefab.name + " (Training Dummy)";
            dummy = instance.GetComponentInChildren<Dummy>();
        }

        if (dummy == null)
            dummy = FindFirstObjectByType<Dummy>();

        if (dummy == null)
        {
            Debug.LogError(
                "TrainingRoom needs a TrainingDummy in the scene or a dummy prefab assigned to TrainingRoomManager.");
            return;
        }

        dummy.Configure(position);
        dummy.DamageChanged += OnDummyDamageChanged;
    }

    private void OnDummyDamageChanged(int totalDamage)
    {
        if (dummyDamageText != null)
            dummyDamageText.text = $"Dummy: {totalDamage}%";
    }

    private void UpdateDebugUI()
    {
        if (playerStateText == null || player == null)
            return;

        string stateName = player.GetCurrentState() != null ? player.GetCurrentState().GetType().Name : "None";
        string animationName = "None";

        if (player.Anim != null && player.Anim.runtimeAnimatorController != null)
        {
            AnimatorClipInfo[] clips = player.Anim.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0 && clips[0].clip != null)
                animationName = clips[0].clip.name;
        }

        playerStateText.text = $"State: {stateName}   Animation: {animationName}";
    }
}
