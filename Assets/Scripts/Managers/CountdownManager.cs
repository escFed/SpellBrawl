using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;
    public CardDrawAnimation cardAnimation;

    [Header("Audio")]
    public AudioSource battleMusic;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNextRound();
    }

    public void StartNextRound()
    {
        StartCoroutine(StartMatchRoutine());
    }

    private IEnumerator StartMatchRoutine()
    {
        DisablePlayers();

        if (cardAnimation != null)
        {
            cardAnimation.PlayDrawAnimation();
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "FIGHT!";

            if (battleMusic != null && !battleMusic.isPlaying)
            {
                battleMusic.Play();
            }
        }

        EnablePlayers();

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void DisablePlayers()
    {
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator player in allPlayers)
            player.SetControlsEnabled(false);
    }

    private void EnablePlayers()
    {
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator player in allPlayers)
            player.SetControlsEnabled(true);
    }
}
