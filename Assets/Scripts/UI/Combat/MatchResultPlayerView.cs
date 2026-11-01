using TMPro;
using UnityEngine;

// References point to persistent Canvas objects. Labels and layout are authored in the prefab.
public sealed class MatchResultPlayerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI overviewHeading;
    [SerializeField] private TextMeshProUGUI detailsHeading;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private UnityEngine.UI.Image favoriteCardIcon;

    [Header("Overview values")]
    [SerializeField] private TextMeshProUGUI eliminationsValue;
    [SerializeField] private TextMeshProUGUI timesEliminatedValue;
    [SerializeField] private TextMeshProUGUI damageDealtValue;
    [SerializeField] private TextMeshProUGUI damageBlockedValue;
    [SerializeField] private TextMeshProUGUI favoriteCardValue;

    [Header("Detail values")]
    [SerializeField] private TextMeshProUGUI damageReceivedValue;
    [SerializeField] private TextMeshProUGUI cardAccuracyValue;
    [SerializeField] private TextMeshProUGUI successfulParriesValue;
    [SerializeField] private TextMeshProUGUI survivalTimeValue;

    public void Show(MatchStatistics statistics, int playerIndex, string heading, bool isWinner, float now)
    {
        MatchStatistics.PlayerRecord record = statistics?.Get(playerIndex);
        if (record == null) return;

        Set(overviewHeading, heading);
        Set(detailsHeading, heading);
        Set(title, statistics.GetTitle(playerIndex, isWinner));
        Set(eliminationsValue, record.Eliminations.ToString());
        Set(timesEliminatedValue, record.TimesEliminated.ToString());
        Set(damageDealtValue, record.DamageDealt.ToString());
        Set(damageBlockedValue, record.DamageBlocked.ToString());
        Set(favoriteCardValue, $"{record.MostUsedCard}  ×{record.MostUsedCardCount}");
        Set(damageReceivedValue, record.DamageReceived.ToString());
        Set(cardAccuracyValue, record.OffensiveCardsUsed > 0 ? $"{record.CardAccuracy}%" : "—");
        Set(successfulParriesValue, record.SuccessfulParries.ToString());
        Set(survivalTimeValue, FormatTime(statistics.SurvivalTime(playerIndex, now)));

        if (favoriteCardIcon != null)
        {
            favoriteCardIcon.sprite = record.MostUsedCardIcon;
            favoriteCardIcon.enabled = record.MostUsedCardIcon != null;
        }
    }

    private static void Set(TextMeshProUGUI label, string value)
    {
        if (label != null) label.text = value;
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(Mathf.Max(0f, seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }
}
