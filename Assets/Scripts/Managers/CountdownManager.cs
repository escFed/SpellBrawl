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
    [SerializeField] private CountdownCameraFocus cameraFocus;

    [Header("Audio")]
    public AudioSource battleMusic;
    [SerializeField] private AudioClip counterClip;
    private AudioSource presenterSource;
    private Coroutine roundStartRoutine;
    private bool firstRoundStarted;
    private bool tutorialOpen;
    public bool IsTutorialOpen => tutorialOpen;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        presenterSource = gameObject.AddComponent<AudioSource>();
        presenterSource.playOnAwake = false;
        presenterSource.spatialBlend = 0f;
        GameSettings.RegisterSource(presenterSource, GameSound.SoundEffects);

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
        bool playIntro = !firstRoundStarted;
        firstRoundStarted = true;
        bool playCardAnimation = roundStartRoutine == null;
        if (roundStartRoutine != null)
        {
            StopCoroutine(roundStartRoutine);
            roundStartRoutine = null;
            cameraFocus?.EndIntro();
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (playIntro)
        {
            roundStartRoutine = StartCoroutine(StartMatchRoutine(playCardAnimation));
            return;
        }

        DisablePlayers();
        if (playCardAnimation && cardAnimation != null)
            cardAnimation.PlayDrawAnimation();
        EnablePlayers();
        if (battleMusic != null && !battleMusic.isPlaying)
            battleMusic.Play();
    }

    public void StopForMatchEnd()
    {
        if (presenterSource != null) presenterSource.Stop();
        if (roundStartRoutine != null)
        {
            StopCoroutine(roundStartRoutine);
            roundStartRoutine = null;
        }
        cameraFocus?.EndIntro();
        DisablePlayers();
        if (battleMusic != null) battleMusic.Stop();
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
            float countStepSeconds = counterClip != null
                ? Mathf.Max(1f, counterClip.length / 3f)
                : 1f;

            countdownText.text = "3";
            if (presenterSource != null && counterClip != null)
                presenterSource.PlayOneShot(counterClip);
            cameraFocus?.FocusOnPlayer(PlayerSlot.PlayerOne);
            yield return new WaitForSeconds(countStepSeconds);

            countdownText.text = "2";
            cameraFocus?.FocusOnPlayer(PlayerSlot.PlayerTwo);
            yield return new WaitForSeconds(countStepSeconds);

            countdownText.text = "1";
            cameraFocus?.ReturnToMatch();
            yield return new WaitForSeconds(countStepSeconds);

            countdownText.text = "FIGHT!";
            cameraFocus?.EndIntro();

            if (battleMusic != null && !battleMusic.isPlaying)
            {
                battleMusic.Play();
            }
        }
        MatchHitStats.ResetStats();
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
        cameraFocus?.EndIntro();
        if (tutorialOpen)
            Time.timeScale = 1f;
    }
}
