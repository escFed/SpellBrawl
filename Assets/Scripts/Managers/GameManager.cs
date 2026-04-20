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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayerDied(int deadPlayerIndex)
    {
        if (isRoundTransitioning) return;

        isRoundTransitioning = true;

        if (deadPlayerIndex == 0)
        {
            p2RoundsWon++;
        }
        else
        {
            p1RoundsWon++;
        }

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

    private IEnumerator ResetRoundRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ResetRoundPositionsAndHealth();
        }

        isRoundTransitioning = false;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
