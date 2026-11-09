using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Rounds")]
    public int roundsToWin = 2;
    private int p1RoundsWon = 0;
    private int p2RoundsWon = 0;

    private bool isRoundTransitioning = false;
    private Coroutine roundResetRoutine;
    private int transitioningRoundWinner = -1;
    public MatchStatistics Statistics { get; private set; }
    public bool MatchFinished { get; private set; }

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
        if (Instance == this)
            MatchHitStats.BeginMatch();

        if (SelectionManager.Instance != null && !SelectionManager.Instance.isTrainingMode)
            roundsToWin = SelectionManager.Instance.Rules.RoundsToWin;

        if (Instance == this && !TryGetComponent<CheatCodes>(out _))
            gameObject.AddComponent<CheatCodes>();
    }

    private void Start()
    {
        UpdateWinsUI();
    }

    public void DebugResetRound()
    {
        if (RespawnManager.Instance == null || CountdownManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] Cannot reset the round without respawn and countdown managers.", this);
            return;
        }

        MatchFinished = false;
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
        UIManager.Instance?.RestoreCombatHudAfterResults();
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
            ShowVictoryScreen(PlayerSlot.PlayerOne);
        }
        else if (p2RoundsWon >= roundsToWin)
        {
            UIManager.Instance?.HideRoundWins();
            ShowVictoryScreen(PlayerSlot.PlayerTwo);
        }
        else
        {
            UIManager.Instance?.ShowRoundWinsForRoundResult();
            roundResetRoutine = StartCoroutine(ResetRoundRoutine());
        }
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

    private async void ShowVictoryScreen(PlayerSlot winner)
    {
        MatchFinished = true;
        CountdownManager.Instance?.StopForMatchEnd();
        UIManager.Instance?.HideCombatHudForResults();
        FindAnyObjectByType<PauseMenu>()?.CloseForMatchEnd();
        CombatFeedback.StopAllRumble();
        AudioListener.pause = false;
        Time.timeScale = 0f;

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

        MatchHitStats.StopRecording();
        await IAWinrateEvent.Report(winner);

        SelectionManager sel = SelectionManager.Instance;
        if (sel != null && !sel.isTrainingMode && sel.matchMode == MatchMode.PlayerVsAI)
        {
            await MatchSummaryEvent.Report(
                MatchHitStats.HitsRegisteredToPlayer,
                MatchHitStats.HitsRegisteredtoAI,
                MatchHitStats.AttemptsToPlayer,
                MatchHitStats.AttemptsToAI,
                MatchHitStats.GetMatchDuration());
            await TimeBetweenCombatStatesEvent.Report(
                MatchHitStats.CombatTimePercentage, MatchHitStats.NeutralTimePercentage);

            RespawnManager respawn = RespawnManager.Instance;
            GameObject[] participantObjects =
            {
                respawn != null ? respawn.p1Instance : null,
                respawn != null ? respawn.p2Instance : null
            };
            foreach (GameObject participantObject in participantObjects)
            {
                CharacterCoordinator participant = participantObject != null
                    ? participantObject.GetComponent<CharacterCoordinator>()
                    : null;
                CharacterDeck participantDeck = participantObject != null
                    ? participantObject.GetComponent<CharacterDeck>()
                    : null;
                if (participant == null || participantDeck == null)
                    continue;

                await CardPickRateEvent.ReportMatch(
                    participantDeck.MatchDeck,
                    participant.Slot,
                    winner == participant.Slot,
                    MatchHitStats.MatchId,
                    participant.Stats != null ? participant.Stats.MetricId : null);
            }

            GameObject winnerObject = respawn != null
                ? winner == PlayerSlot.PlayerOne ? respawn.p1Instance : respawn.p2Instance
                : null;
            CharacterHealth winnerHealth = winnerObject != null
                ? winnerObject.GetComponent<CharacterHealth>()
                : null;
            if (winnerHealth != null)
                await AveragePlayersLifesEvents.Report(winnerHealth.fallLives,
                    sel.GetControlMode(winner) == PlayerMode.AI);
        }

        RespawnManager matchRespawn = RespawnManager.Instance;
        float durationSeconds = MatchHitStats.GetMatchDuration();
        CharacterCoordinator playerOneCharacter = matchRespawn != null && matchRespawn.p1Instance != null
            ? matchRespawn.p1Instance.GetComponent<CharacterCoordinator>()
            : null;
        CharacterCoordinator playerTwoCharacter = matchRespawn != null && matchRespawn.p2Instance != null
            ? matchRespawn.p2Instance.GetComponent<CharacterCoordinator>()
            : null;
        await MatchSummaryEvent.ReportCharacterDuration(
            playerOneCharacter, winner == PlayerSlot.PlayerOne, durationSeconds, MatchHitStats.MatchId);
        await MatchSummaryEvent.ReportCharacterDuration(
            playerTwoCharacter, winner == PlayerSlot.PlayerTwo, durationSeconds, MatchHitStats.MatchId);

        await FirstCardUsedTime.WaitForPendingReportAsync();

        MetricsConsentManager metrics = MetricsConsentManager.Instance;
        if (metrics != null)
            await metrics.FlushAsync();

    }

    private static string GetWinnerMessage(PlayerSlot slot)
    {
        string winnerName = SelectionManager.Instance != null ? SelectionManager.Instance.GetDisplayName(slot) : ModeRules.GetDisplayName(MatchMode.PlayerVsPlayer, slot);
        return $"{winnerName} WINS";


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
