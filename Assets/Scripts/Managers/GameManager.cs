using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Rounds")]
    public int roundsToWin = 2;
    private int p1RoundsWon = 0;
    private int p2RoundsWon = 0;

    private bool isRoundTransitioning = false;

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
    }

    private void Start()
    {
        UpdateWinsUI();
    }

    public void PlayerDied(int deadPlayerIndex)
    {
        if (isRoundTransitioning) return;

        isRoundTransitioning = true;

        TargetGroup camUpdater = FindAnyObjectByType<TargetGroup>();
        if (camUpdater != null)
        {
            camUpdater.RefreshTargets();
        }

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

        if (p1RoundsWon >= roundsToWin)
        {
            ShowVictoryScreen("Player 1 Wins");
        }
        else if (p2RoundsWon >= roundsToWin)
        {
            ShowVictoryScreen("AI Wins");
        }
        else
        {
            StartCoroutine(ResetRoundRoutine());
        }
    }

    public void UpdateWinsUI()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.p1_winsText != null)
                UIManager.Instance.p1_winsText.text = p1Wins.ToString(); ;
            if (UIManager.Instance.p2_winsText != null)
                UIManager.Instance.p2_winsText.text = p2Wins.ToString(); ;
        }
    }

    private IEnumerator ResetRoundRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ResetRoundPositionsAndHealth();
        }

        isRoundTransitioning = false;

        CountdownManager.Instance.StartNextRound();
    }

    private void ShowVictoryScreen(string message)
    {
        if (winnerText != null) winnerText.text = message;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Rematch()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
