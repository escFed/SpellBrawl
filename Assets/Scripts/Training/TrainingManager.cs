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
    private CharacterDatabase database;
    [SerializeField, Range(0, 9999)] private int startingDamage;
    public int StartingDamage => startingDamage;
    public Dummy TrainingDummy => dummy;
    public CharacterDatabase Database => database;
    public bool CanEditSession => Application.isPlaying && player != null && dummy != null && !PauseMenu.isPaused;
    private static readonly int[] DamagePresets = { 0, 50, 100, 150, 200 };
    private static readonly Vector2[] InfluencePresets = { Vector2.zero, Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        database = GetCharacterDatabase();
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
            helpText.text = "R: Repeat | F1: Damage | F2: Defender | F3: DI | ESC: Menu";

        ResetTraining();
    }

    private void Update()
    {
        if (PauseMenu.isPaused)
            return;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.f1Key.wasPressedThisFrame) CycleStartingDamage();
            if (Keyboard.current.f2Key.wasPressedThisFrame) CycleDefender();
            if (Keyboard.current.f3Key.wasPressedThisFrame) CycleInfluence();
            if (Keyboard.current.rKey.wasPressedThisFrame)
                ResetTraining();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ReturnToMainMenu();
        }

        UpdateDebugUI();
    }

    public void SetStartingDamage(int damage)
    {
        startingDamage = Mathf.Clamp(damage, 0, 9999);
        if (CanEditSession) ResetTraining();
    }

    public void SetDefender(CharacterStats stats)
    {
        if (!CanEditSession || stats == null) return;
        dummy.UseCharacterStats(stats);
        ResetTraining();
    }

    public void SetInfluence(Vector2 direction)
    {
        if (!CanEditSession) return;
        dummy.DirectionalInput = direction;
        ResetTraining();
    }

    public void CycleStartingDamage()
    {
        int index = System.Array.IndexOf(DamagePresets, startingDamage);
        SetStartingDamage(DamagePresets[(index + 1) % DamagePresets.Length]);
    }

    public void CycleDefender()
    {
        if (!CanEditSession || database == null || database.CharacterCount == 0) return;
        int index = database.Characters.IndexOf(dummy.TargetCharacter);
        SetDefender(database.GetCharacter((index + 1) % database.CharacterCount));
    }

    public void CycleInfluence()
    {
        if (!CanEditSession) return;
        int index = System.Array.IndexOf(InfluencePresets, dummy.DirectionalInput);
        SetInfluence(InfluencePresets[(index + 1) % InfluencePresets.Length]);
    }

    // A repeatable impact for calibration; it intentionally bypasses hitbox detection and attack timing.
    public bool ApplyTestHit(AttackStats attack, float chargeRatio)
    {
        if (!CanEditSession || attack == null) return false;
        ResetTraining();
        return dummy.ReceiveHit(CreateTestHit(attack, chargeRatio, dummy.transform.position));
    }

    public static CombatHit CreateTestHit(AttackStats attack, float chargeRatio, Vector2 point)
    {
        if (attack is NormalAttackStats normal)
            return new CombatHit(normal.damage, normal.knockback, normal.hitStun, normal.hitReaction, point, -1, normal.launch);
        if (attack is HeavyAttackStats heavy)
        {
            float ratio = Mathf.Clamp01(chargeRatio);
            Vector2 direction = heavy.knockbackDirection.sqrMagnitude > 0.0001f ? heavy.knockbackDirection.normalized : Vector2.right;
            return new CombatHit(Mathf.RoundToInt(HeavyAttackCharge.CalculateDamage(heavy.minDamage, heavy.maxDamage, ratio)),
                direction * HeavyAttackCharge.CalculateKnockback(heavy.minKnockback, heavy.maxKnockback, ratio),
                HeavyAttackCharge.CalculateHitStun(heavy.hitStun, heavy.maxHitStun, ratio), heavy.hitReaction, point, -1,
                heavy.launch, Mathf.Lerp(heavy.launch?.growth ?? 3f, heavy.maxKnockbackGrowth, ratio));
        }
        return new CombatHit(0, Vector2.zero);
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
        dummy?.SetDamage(startingDamage);

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
        if (dummy.TargetCharacter == null && player != null)
            dummy.UseCharacterStats(player.stats);
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
