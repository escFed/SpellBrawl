using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

// The Canvas owns every stage and timing setting; match values stay in GameManager.Statistics.
public sealed class MatchResultsView : MonoBehaviour
{
    [Header("Sequence stages (Canvas children)")]
    [SerializeField] private GameObject gameStage;
    [SerializeField] private GameObject animationStage;
    [SerializeField] private GameObject winnerStage;
    [SerializeField] private GameObject statisticsStage;
    [SerializeField] private CanvasGroup transitionCurtain;

    [Header("Sequence timing (unscaled seconds)")]
    [SerializeField, Min(0f)] private float gameHoldSeconds = 1.25f;
    [SerializeField, Min(0f)] private float transitionSeconds = 0.35f;
    [SerializeField, Min(0f)] private float placeholderAnimationSeconds = 1.2f;
    [SerializeField, Min(0f)] private float winnerHoldSeconds = 1.6f;

    [Header("Winner stage")]
    [SerializeField] private TextMeshProUGUI winnerIdentity;

    [Header("Victory animation stage")]
    [SerializeField] private UnityEngine.UI.Image winnerPortrait;
    [SerializeField] private UnityEngine.UI.Image amiroRune;
    [SerializeField] private Animator victoryAnimator;
    [SerializeField] private AnimationClip amiroVictoryClip;
    [SerializeField] private Sprite[] amiroVictoryFrames;
    [SerializeField] private Sprite[] amiroIdleFrames;
    [SerializeField] private Sprite[] amiroRuneFrames;

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

    public void Show(MatchStatistics statistics, int winnerIndex)
    {
        if (statistics == null) return;
        StopSequence();

        for (int i = 0; i < 2; i++)
        {
            string playerName = GetPlayerName(i);
            CharacterStats character = GetCharacter(i);
            string heading = character != null ? $"{playerName}  ·  {character.characterName}" : playerName;
            if (playerPanels != null && i < playerPanels.Length && playerPanels[i] != null)
                playerPanels[i].Show(statistics, i, heading, i == winnerIndex, Time.time);
        }

        CharacterStats winner = GetCharacter(winnerIndex);
        string characterName = winner != null ? winner.characterName : "PERSONAJE";
        if (winnerIdentity != null)
            winnerIdentity.text = $"{characterName}\n{GetPlayerName(winnerIndex)}";

        isAmiro = winner != null && string.Equals(characterName, "AMIRO", System.StringComparison.OrdinalIgnoreCase);
        if (winnerPortrait != null)
        {
            winnerPortrait.sprite = isAmiro && amiroVictoryFrames != null && amiroVictoryFrames.Length > 0
                ? amiroVictoryFrames[0]
                : winner != null ? winner.characterIcon : null;
            winnerPortrait.enabled = winnerPortrait.sprite != null;
            winnerPortrait.rectTransform.localScale = isAmiro ? Vector3.one * 0.35f : Vector3.one;
        }
        if (amiroRune != null)
        {
            amiroRune.gameObject.SetActive(isAmiro && amiroRuneFrames != null && amiroRuneFrames.Length > 0);
            if (amiroRune.gameObject.activeSelf) amiroRune.sprite = amiroRuneFrames[0];
            amiroRune.rectTransform.localScale = Vector3.one * 0.1f;
            amiroRune.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -120f);
        }

        SetDetails(false);
        SetStage(gameStage != null ? gameStage : statisticsStage);
        if (transitionCurtain != null) transitionCurtain.alpha = 0f;
        presentationTime = 0f;
        animationPlaying = false;

        if (Application.isPlaying && gameStage != null && animationStage != null &&
            winnerStage != null && statisticsStage != null)
            sequence = StartCoroutine(PlaySequence());
    }

    public void ToggleDetails() => SetDetails(details != null && !details.activeSelf);

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSecondsRealtime(gameHoldSeconds);
        yield return TransitionTo(animationStage);

        presentationTime = 0f;
        animationPlaying = true;
        StartVictoryClip();
        float animationSeconds = playingClip ? amiroVictoryClip.length : placeholderAnimationSeconds;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, animationSeconds));
        animationPlaying = false;
        StopVictoryClip();

        yield return TransitionTo(winnerStage);
        yield return new WaitForSecondsRealtime(winnerHoldSeconds);
        yield return TransitionTo(statisticsStage);
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
        if (animationStage != null) animationStage.SetActive(animationStage == active);
        if (winnerStage != null) winnerStage.SetActive(winnerStage == active);
        if (statisticsStage != null) statisticsStage.SetActive(statisticsStage == active);
    }

    private void SetDetails(bool showDetails)
    {
        if (overview != null) overview.SetActive(!showDetails);
        if (details != null) details.SetActive(showDetails);
        if (detailsButtonLabel != null) detailsButtonLabel.text = showDetails ? "RESUMEN" : "DETALLES";
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
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
        animationPlaying = false;
        StopVictoryClip();
    }

    private void OnDisable() => StopSequence();

    private void Update()
    {
        if (!isAmiro || !animationPlaying || playingClip || animationStage == null || !animationStage.activeInHierarchy ||
            winnerPortrait == null) return;

        presentationTime += Time.unscaledDeltaTime;
        float entrance = Mathf.Clamp01(presentationTime / 0.55f);
        float overshoot = 1f + Mathf.Sin(entrance * Mathf.PI) * 0.12f;
        winnerPortrait.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.35f, overshoot, entrance);

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
        amiroRune.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.1f, 1f, entrance);
        amiroRune.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (1f - entrance) * -120f);
    }

    private static string GetPlayerName(int playerIndex) => SelectionManager.Instance != null
        ? SelectionManager.Instance.GetDisplayName((PlayerSlot)playerIndex)
        : $"JUGADOR {playerIndex + 1}";

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
