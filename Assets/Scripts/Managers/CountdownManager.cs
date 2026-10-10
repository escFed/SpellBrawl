using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;
    public CardDrawAnimation cardAnimation;
    [SerializeField] private MatchTutorialView tutorialView;

    [Header("Audio")]
    public AudioSource battleMusic;
    private Coroutine roundStartRoutine;
    private bool tutorialOpen;
    public bool IsTutorialOpen => tutorialOpen;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // The panel may be left visible in the editor for Canvas layout work.
        if (tutorialView != null)
            tutorialView.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(BeginFirstRound());
    }

    private IEnumerator BeginFirstRound()
    {
        // RespawnManager and CharacterDeck create and publish the initial HUD in Start.
        yield return null;
        DisablePlayers();

        if (GameplayAidSettings.ShowMatchTutorial)
        {
            if (tutorialView != null && tutorialView.IsConfigured && UIManager.Instance != null)
            {
                foreach (CharacterDeck deck in FindObjectsByType<CharacterDeck>(FindObjectsSortMode.None))
                    deck.RefreshUI();

                // The tutorial uses its own editable card images; the real hand
                // appears when the normal draw animation runs after the countdown.
                UIManager.Instance.HideCardsForTutorial();

                tutorialView.gameObject.SetActive(true);
                Time.timeScale = 0f;
                tutorialOpen = true;
                tutorialView.Open(UIManager.Instance, FinishTutorial);
                yield break;
            }

            Debug.LogError("[CountdownManager] Assign the Canvas tutorial and its steps in the Inspector.", this);
        }

        StartNextRound();
    }

    private void FinishTutorial(bool hideFutureTutorials)
    {
        if (hideFutureTutorials)
        {
            GameplayAidSettings.SetShowMatchTutorial(false);
            PlayerPrefs.Save();
        }

        tutorialView.Close();
        tutorialOpen = false;

        UIFocus.Clear();
        Time.timeScale = 1f;
        StartNextRound();
    }

#if UNITY_EDITOR
    [ContextMenu("Mostrar tutorial en la próxima prueba")]
    private void ShowTutorialNextTest()
    {
        GameplayAidSettings.SetShowMatchTutorial(true);
        PlayerPrefs.Save();
        Debug.Log("[CountdownManager] El tutorial volverá a mostrarse al iniciar la próxima partida.", this);
    }
#endif

    public void StartNextRound()
    {
        bool restartingCountdown = roundStartRoutine != null;
        if (restartingCountdown)
            StopCoroutine(roundStartRoutine);

        roundStartRoutine = StartCoroutine(StartMatchRoutine(!restartingCountdown));
    }

    private IEnumerator StartMatchRoutine(bool playCardAnimation)
    {
        DisablePlayers();

        if (playCardAnimation && cardAnimation != null)
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
        roundStartRoutine = null;
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

    private void OnDestroy()
    {
        if (tutorialOpen)
            Time.timeScale = 1f;
    }
}
