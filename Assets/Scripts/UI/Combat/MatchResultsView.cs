using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

// The Canvas owns the screens; match values stay in GameManager.Statistics.
public sealed class MatchResultsView : MonoBehaviour
{
    [Header("Sequence stages (Canvas children)")]
    [SerializeField] private GameObject gameStage;
    [SerializeField] private GameObject loadingStage;
    [SerializeField] private GameObject winnerStage;
    [SerializeField] private GameObject statisticsStage;
    [SerializeField] private CanvasGroup transitionCurtain;
    [SerializeField] private UnityEngine.UI.Image panelBackground;

    [Header("Sequence timing (unscaled seconds)")]
    [SerializeField, Min(0f)] private float gameHoldSeconds = 1.25f;
    [SerializeField, Min(0f)] private float loadingSeconds = 2f;
    [SerializeField, Min(0f)] private float transitionSeconds = 0.35f;

    [Header("Winner stage")]
    [SerializeField] private TextMeshProUGUI winnerIdentity;
    [SerializeField] private TextMeshProUGUI winnerCharacter;
    [SerializeField] private UnityEngine.UI.Button nextButton;

    [Header("Victory soundtrack")]
    [SerializeField] private AudioSource victoryMusic;

    [Header("Presenter voice")]
    [SerializeField] private AudioClip gameClip;
    [SerializeField] private AudioClip winnerClip;

    [Header("Winner animation")]
    [SerializeField] private UnityEngine.UI.Image winnerPortrait;
    [SerializeField] private UnityEngine.UI.Image amiroRune;
    [SerializeField] private Animator victoryAnimator;
    [SerializeField] private AnimationClip amiroVictoryClip;
    [SerializeField] private Sprite[] amiroVictoryFrames;
    [SerializeField] private Sprite[] amiroIdleFrames;
    [SerializeField] private Sprite[] amiroRuneFrames;
    [SerializeField] private Material blueAmiroMaterial;

    [Header("Statistics stage")]
    [SerializeField] private GameObject overview;
    [SerializeField] private MatchResultPlayerView[] playerPanels = new MatchResultPlayerView[2];
    [SerializeField] private GameObject details;
    [SerializeField] private TextMeshProUGUI detailsButtonLabel;

    private Coroutine sequence;
    private PlayableGraph victoryGraph;
    private bool isAmiro;
    private bool playingClip;
    private bool animationPlaying;
    private float presentationTime;
    private AudioSource presenterSource;

    private void Awake()
    {
        // Victory music already has an AudioSource on this object; the voice needs
        // its own object so AudioController can assign a different volume channel.
        GameObject presenterPlayer = new GameObject("Winner Voice");
        presenterPlayer.transform.SetParent(transform, false);
        presenterSource = presenterPlayer.AddComponent<AudioSource>();
        presenterSource.playOnAwake = false;
        presenterSource.spatialBlend = 0f;
        GameSettings.RegisterSource(presenterSource, GameSound.SoundEffects);
        if (victoryMusic != null)
            GameSettings.RegisterSource(victoryMusic, GameSound.Music);
    }

    public void Show(MatchStatistics statistics, int winnerIndex)
    {
        if (statistics == null) return;
        StopSequence();
        if (victoryMusic != null) victoryMusic.Stop();

        for (int i = 0; i < 2; i++)
        {
            string playerName = GetPlayerName(i);
            CharacterStats character = GetCharacter(i);
            string heading = character != null ? $"{playerName}  ·  {character.characterName}" : playerName;
            if (playerPanels != null && i < playerPanels.Length && playerPanels[i] != null)
                playerPanels[i].Show(statistics, i, heading, i == winnerIndex, Time.time);
        }

        CharacterStats winner = GetCharacter(winnerIndex);
        string characterName = winner != null ? winner.characterName : "CHARACTER";
        if (winnerIdentity != null) winnerIdentity.text = GetPlayerName(winnerIndex);
        if (winnerCharacter != null) winnerCharacter.text = characterName;

        isAmiro = winner != null && string.Equals(characterName, "AMIRO", System.StringComparison.OrdinalIgnoreCase);
        SelectionManager selection = SelectionManager.Instance;
        bool blueAmiroWinner = isAmiro &&
            (winnerIndex == (int)PlayerSlot.PlayerTwo ||
             selection != null && selection.GetControlMode((PlayerSlot)winnerIndex) == PlayerMode.AI);
        Material victoryMaterial = blueAmiroWinner ? blueAmiroMaterial : null;
        if (winnerPortrait != null)
        {
            winnerPortrait.material = victoryMaterial;
            winnerPortrait.sprite = isAmiro && amiroVictoryFrames != null && amiroVictoryFrames.Length > 0
                ? amiroVictoryFrames[0]
                : winner != null ? winner.characterIcon : null;
            winnerPortrait.enabled = winnerPortrait.sprite != null;
        }
        if (amiroRune != null)
        {
            amiroRune.material = victoryMaterial;
            amiroRune.gameObject.SetActive(isAmiro && amiroVictoryClip == null &&
                amiroRuneFrames != null && amiroRuneFrames.Length > 0);
            if (amiroRune.gameObject.activeSelf) amiroRune.sprite = amiroRuneFrames[0];
        }

        SetDetails(false);
        SetStage(gameStage != null ? gameStage : loadingStage);
        if (gameStage != null && presenterSource != null && gameClip != null)
            presenterSource.PlayOneShot(gameClip);
        if (panelBackground != null) panelBackground.enabled = gameStage == null;
        if (transitionCurtain != null) transitionCurtain.alpha = 0f;
        if (nextButton != null) nextButton.interactable = true;
        presentationTime = 0f;
        animationPlaying = false;

        if (Application.isPlaying && loadingStage != null && winnerStage != null && statisticsStage != null)
            sequence = StartCoroutine(PlaySequence());
    }

    public void ToggleDetails() => SetDetails(details != null && !details.activeSelf);

    public void Next()
    {
        if (!Application.isPlaying || sequence != null || winnerStage == null || !winnerStage.activeInHierarchy)
            return;
        if (nextButton != null) nextButton.interactable = false;
        sequence = StartCoroutine(ShowStatistics());
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSecondsRealtime(gameHoldSeconds);
        SetStage(loadingStage);
        if (panelBackground != null) panelBackground.enabled = true;
        yield return new WaitForSecondsRealtime(loadingSeconds);
        yield return TransitionTo(winnerStage);
        if (presenterSource != null && winnerClip != null)
            presenterSource.PlayOneShot(winnerClip);
        presentationTime = 0f;
        animationPlaying = true;
        StartVictoryClip();
        UIFocus.SelectFirst(winnerStage);
        if (winnerClip != null)
            yield return new WaitForSecondsRealtime(winnerClip.length);
        if (victoryMusic != null && victoryMusic.clip != null) victoryMusic.Play();
        sequence = null;
    }

    private IEnumerator ShowStatistics()
    {
        yield return TransitionTo(statisticsStage);
        animationPlaying = false;
        StopVictoryClip();
        UIFocus.SelectFirst(statisticsStage);
        sequence = null;
    }

    private IEnumerator TransitionTo(GameObject nextStage)
    {
        if (transitionCurtain != null)
            yield return FadeCurtain(0f, 1f);
        SetStage(nextStage);
        if (transitionCurtain != null)
            yield return FadeCurtain(1f, 0f);
    }

    private IEnumerator FadeCurtain(float from, float to)
    {
        if (transitionSeconds <= 0f)
        {
            transitionCurtain.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        transitionCurtain.alpha = from;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            transitionCurtain.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / transitionSeconds));
            yield return null;
        }
        transitionCurtain.alpha = to;
    }

    private void SetStage(GameObject active)
    {
        if (gameStage != null) gameStage.SetActive(gameStage == active);
        if (loadingStage != null) loadingStage.SetActive(loadingStage == active);
        if (winnerStage != null) winnerStage.SetActive(winnerStage == active);
        if (statisticsStage != null) statisticsStage.SetActive(statisticsStage == active);
    }

    private void SetDetails(bool showDetails)
    {
        if (overview != null) overview.SetActive(!showDetails);
        if (details != null) details.SetActive(showDetails);
        if (detailsButtonLabel != null) detailsButtonLabel.text = showDetails ? "OVERVIEW" : "DETAILS";
    }

    private void StartVictoryClip()
    {
        if (!isAmiro || amiroVictoryClip == null || victoryAnimator == null) return;

        victoryGraph = PlayableGraph.Create("MatchVictory");
        victoryGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
        AnimationClipPlayable playable = AnimationClipPlayable.Create(victoryGraph, amiroVictoryClip);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(victoryGraph, "Victory", victoryAnimator);
        output.SetSourcePlayable(playable);
        victoryGraph.Play();
        playingClip = true;
    }

    private void StopVictoryClip()
    {
        if (victoryGraph.IsValid()) victoryGraph.Destroy();
        playingClip = false;
    }

    private void StopSequence()
    {
        if (presenterSource != null) presenterSource.Stop();
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
        animationPlaying = false;
        StopVictoryClip();
    }

    private void OnDisable()
    {
        StopSequence();
        if (victoryMusic != null) victoryMusic.Stop();
        if (gameStage != null) gameStage.SetActive(false);
    }

    private void Update()
    {
        if (!isAmiro || !animationPlaying || playingClip || winnerStage == null || !winnerStage.activeInHierarchy ||
            winnerPortrait == null) return;

        presentationTime += Time.unscaledDeltaTime;

        if (amiroVictoryFrames != null && amiroVictoryFrames.Length > 0 && presentationTime < 0.9f)
            winnerPortrait.sprite = amiroVictoryFrames[Mathf.Min(amiroVictoryFrames.Length - 1,
                Mathf.FloorToInt(presentationTime * 12f))];
        else if (amiroIdleFrames != null && amiroIdleFrames.Length > 0)
            winnerPortrait.sprite = amiroIdleFrames[Mathf.FloorToInt(Mathf.Max(0f, presentationTime - 0.9f) * 8f) % amiroIdleFrames.Length];

        if (amiroRune == null || !amiroRune.gameObject.activeSelf) return;
        if (amiroRuneFrames != null && amiroRuneFrames.Length > 0)
        {
            int frame = Mathf.Min(amiroRuneFrames.Length - 1, Mathf.FloorToInt(presentationTime * 12f));
            amiroRune.sprite = amiroRuneFrames[frame];
        }
    }

    private static string GetPlayerName(int playerIndex) => SelectionManager.Instance != null
        ? SelectionManager.Instance.GetDisplayName((PlayerSlot)playerIndex)
        : $"PLAYER {playerIndex + 1}";

    private static CharacterStats GetCharacter(int playerIndex)
    {
        SelectionManager selection = SelectionManager.Instance;
        if (selection == null || selection.characterDb == null) return null;
        int index = selection.GetSelectedCharacter((PlayerSlot)playerIndex);
        return index >= 0 && index < selection.characterDb.CharacterCount
            ? selection.characterDb.GetCharacter(index)
            : null;
    }

}
