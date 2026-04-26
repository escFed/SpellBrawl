using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public TextMeshProUGUI winnerText;

    [Header("Card Selection")]
    public List<UICardData> allCards;
    public int cardsperPlayer = 20;

  
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayerDied(int deadPlayerIndex)
    {
        int winnerNumber = deadPlayerIndex == 0 ? 2 : 1;

        if (winnerText != null)
        {
            winnerText.text = "¡Player " + winnerNumber + " Wins!";
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

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

    public void OnPlayerCardsSelected()
    {
        StartMatch(); 
     
    }


    public void StartMatch()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            if (p.GetComponent<PlayerAI>() == null)
                p.SetInputEnabled(true);  // jugador humano
            else
                p.GetComponent<PlayerAI>().SetInputEnabled(true);  // IA
        }

        List<UICardData> AIcards = GetAICards(cardsperPlayer);
    }


    private List<UICardData> GetAICards(int amount)
    {
        List<UICardData> pool = new List<UICardData>(allCards);
        List<UICardData> result = new();

        amount = Mathf.Min(amount, pool.Count);
        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);

        }

        return result;
    }







}
