using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Rounds")]
    public int roundsToWin = 2;
    private int p1RoundsWon = 0;
    private int p2RoundsWon = 0;

    [Header("Combat Time Metric")]
    [SerializeField, Min(0f)] private float combatDetectionRadius = 5f;
    private float trackedMatchTime;
    private float timeInCombatRange;
    private bool hasCombatMetricSamples;
    private bool combatMetricReported;

    private bool isRoundTransitioning = false;
    private Coroutine roundResetRoutine;
    private int transitioningRoundWinner = -1;
    public MatchStatistics Statistics { get; private set; }

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public TextMeshProUGUI winnerText;

    [Header("RoundsPoint")]
    public int p1Wins = 0;
    public int p2Wins = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Statistics = new MatchStatistics();

        if (SelectionManager.Instance != null && !SelectionManager.Instance.isTrainingMode)
            roundsToWin = SelectionManager.Instance.Rules.RoundsToWin;

        if (Instance == this && !TryGetComponent<CheatCodes>(out _))
            gameObject.AddComponent<CheatCodes>();
    }

    private void Start()
    {
        UpdateWinsUI();
    }

    private void Update()
    {
        if (combatMetricReported || isRoundTransitioning || PauseMenu.isPaused ||
            !TryGetCombatMetricParticipants(out CharacterCoordinator ai, out CharacterCoordinator player))
            return;

        if (ai.IsDead || player.IsDead || ai.Controller == null ||
            player.Controller == null || !ai.Controller.InputEnabled || !player.Controller.InputEnabled)
            return;

        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        trackedMatchTime += deltaTime;
        hasCombatMetricSamples = true;

        float radius = Mathf.Max(0f, combatDetectionRadius);
        if ((ai.transform.position - player.transform.position).sqrMagnitude <= radius * radius)
            timeInCombatRange += deltaTime;
    }

    public void DebugResetRound()
    {
        if (RespawnManager.Instance == null || CountdownManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] Cannot reset the round without respawn and countdown managers.", this);
            return;
        }

        if (roundResetRoutine != null)
        {
            StopCoroutine(roundResetRoutine);
            roundResetRoutine = null;
        }

        // If a knockout was already counted, restart that same round.
        if (isRoundTransitioning && transitioningRoundWinner == 0)
        {
            p1RoundsWon = Mathf.Max(0, p1RoundsWon - 1);
            p1Wins = Mathf.Max(0, p1Wins - 1);
        }
        else if (isRoundTransitioning && transitioningRoundWinner == 1)
        {
            p2RoundsWon = Mathf.Max(0, p2RoundsWon - 1);
            p2Wins = Mathf.Max(0, p2Wins - 1);
        }

        transitioningRoundWinner = -1;
        isRoundTransitioning = false;
        UpdateWinsUI();
        UIManager.Instance?.HideRoundWins();
        if (PauseMenu.isPaused)
        {
            PauseMenu pauseMenu = FindAnyObjectByType<PauseMenu>();
            if (pauseMenu != null)
                pauseMenu.ResumeGame();
            else
                PauseMenu.isPaused = false;
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        UIFocus.Clear();
        Time.timeScale = 1f;
        AudioListener.pause = false;

        RespawnManager.Instance.ResetRoundPositionsAndHealth();
        CountdownManager.Instance.StartNextRound();
        Debug.Log("[Cheat] Round reset; score preserved.", this);
    }

    public void PlayerDied(int deadPlayerIndex)
    {
        if (isRoundTransitioning) return;

        isRoundTransitioning = true;
        transitioningRoundWinner = deadPlayerIndex == 0 ? 1 : 0;

        if (deadPlayerIndex == 0)
        {
            p2RoundsWon++;
        }
        else
        {
            p1RoundsWon++;
        }

        if (deadPlayerIndex == 0) p2Wins++;
        else p1Wins++;

        UpdateWinsUI();
        Statistics.RecordRoundScore(p1RoundsWon, p2RoundsWon);
        Statistics.EndLife(1 - deadPlayerIndex, Time.time);


        if (p1RoundsWon >= roundsToWin)
        {
            UIManager.Instance?.HideRoundWins();
            ReportCombatTimeMetric();

            var p1Object = RespawnManager.Instance != null
                ? RespawnManager.Instance.p1Instance
                : null;
            var p1Health = p1Object != null
                ? p1Object.GetComponent<CharacterHealth>()
                : null;

            if (p1Health != null)
            {
                AveragePlayersLifesEvents.Report(p1Health.fallLives, IsAI(PlayerSlot.PlayerOne));
            }
            else
            {
                Debug.LogWarning("[GameManager] No se pudieron reportar las vidas restantes de P1: falta CharacterHealth.", this);
            }

            ShowVictoryScreen(PlayerSlot.PlayerOne);
        }
        else if (p2RoundsWon >= roundsToWin)
        {
            UIManager.Instance?.HideRoundWins();
            ReportCombatTimeMetric();

            var p2Object = RespawnManager.Instance != null
                ? RespawnManager.Instance.p2Instance
                : null;
            var p2Health = p2Object != null
                ? p2Object.GetComponent<CharacterHealth>()
                : null;

            if (p2Health != null)
            {
                AveragePlayersLifesEvents.Report(p2Health.fallLives, IsAI(PlayerSlot.PlayerTwo));
            }
            else
            {
                Debug.LogWarning("[GameManager] No se pudieron reportar las vidas restantes de P2: falta CharacterHealth.", this);
            }

            ShowVictoryScreen(PlayerSlot.PlayerTwo);
        }
        else
        {
            UIManager.Instance?.ShowRoundWinsForRoundResult();
            roundResetRoutine = StartCoroutine(ResetRoundRoutine());
        }



    }

    private bool TryGetCombatMetricParticipants(out CharacterCoordinator ai, out CharacterCoordinator player)
    {
        ai = null;
        player = null;

        RespawnManager respawn = RespawnManager.Instance;
        if (respawn == null || respawn.p1Instance == null || respawn.p2Instance == null)
            return false;

        if (!respawn.p1Instance.TryGetComponent(out CharacterCoordinator p1) ||
            !respawn.p2Instance.TryGetComponent(out CharacterCoordinator p2))
            return false;

        if (p1.Mode == PlayerMode.AI && p2.Mode == PlayerMode.Player)
        {
            ai = p1;
            player = p2;
        }
        else if (p2.Mode == PlayerMode.AI && p1.Mode == PlayerMode.Player)
        {
            ai = p2;
            player = p1;
        }

        return ai != null && player != null;
    }

    private void ReportCombatTimeMetric()
    {
        if (combatMetricReported || !TryGetCombatMetricParticipants(out _, out _))
            return;

        combatMetricReported = true;
        float denominator = trackedMatchTime;
        if (!hasCombatMetricSamples || denominator <= 0f)
        {
            Debug.LogWarning("[GameManager] Combat time was not reported because there was not enough active match time to calculate a percentage.", this);
            return;
        }

        float combatPercentage = Mathf.Clamp(timeInCombatRange / denominator * 100f, 0f, 100f);
        float neutralPercentage = Mathf.Clamp((denominator - timeInCombatRange) / denominator * 100f, 0f, 100f);
        TimeBetweenCombatStatesEvent.Report(combatPercentage, neutralPercentage);
    }

    public void UpdateWinsUI()
    {
        if (UIManager.Instance != null && UIManager.Instance.roundScoreText != null)
            UIManager.Instance.roundScoreText.text = $"{p1Wins} - {p2Wins}";
    }

    private IEnumerator ResetRoundRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ResetRoundPositionsAndHealth();
        }

        isRoundTransitioning = false;
        transitioningRoundWinner = -1;
        roundResetRoutine = null;

        CountdownManager.Instance.StartNextRound();
    }
    private static bool IsAI(PlayerSlot slot) =>
        SelectionManager.Instance != null &&
        SelectionManager.Instance.GetControlMode(slot) == PlayerMode.AI;
    private void ShowVictoryScreen(PlayerSlot winner)
    {

        IAWinrateEvent.Report(winner);
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            MatchResultsView results = victoryPanel.GetComponent<MatchResultsView>();
            if (results != null)
                results.Show(Statistics, (int)winner);
            else
            {
                if (winnerText != null) winnerText.text = GetWinnerMessage(winner);
                UIFocus.SelectFirst(victoryPanel);
            }
        }
        Time.timeScale = 0f;
    }

    private static string GetWinnerMessage(PlayerSlot slot)
    {
        string winnerName = SelectionManager.Instance != null ? SelectionManager.Instance.GetDisplayName(slot): ModeRules.GetDisplayName(MatchMode.PlayerVsPlayer, slot);
        return $"{winnerName} GANA";
    }

    public void Rematch()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        LoadingScreen.LoadStage(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
