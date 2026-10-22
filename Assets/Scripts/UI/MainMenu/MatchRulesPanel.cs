using TMPro;
using UnityEngine;

public class MatchRulesPanel : MonoBehaviour
{
    [Header("Canvas labels")]
    [SerializeField] private TextMeshProUGUI stocksValue;
    [SerializeField] private TextMeshProUGUI roundsValue;
    [SerializeField] private TextMeshProUGUI initialHandValue;
    [SerializeField] private TextMeshProUGUI maxHandValue;

    private MatchRules Rules => SelectionManager.Instance != null
        ? SelectionManager.Instance.Rules
        : null;

    private void OnEnable()
    {
        Refresh();
    }

    public void IncreaseStocks() => ChangeStocks(1);
    public void DecreaseStocks() => ChangeStocks(-1);
    public void IncreaseRounds() => ChangeRounds(1);
    public void DecreaseRounds() => ChangeRounds(-1);
    public void IncreaseInitialHand() => ChangeInitialHand(1);
    public void DecreaseInitialHand() => ChangeInitialHand(-1);
    public void IncreaseMaxHand() => ChangeMaxHand(1);
    public void DecreaseMaxHand() => ChangeMaxHand(-1);

    public void Refresh()
    {
        MatchRules rules = Rules;
        if (rules == null)
            return;

        if (stocksValue != null) stocksValue.text = rules.Stocks.ToString();
        if (roundsValue != null) roundsValue.text = rules.RoundsToWin.ToString();
        if (initialHandValue != null) initialHandValue.text = rules.InitialHandSize.ToString();
        if (maxHandValue != null) maxHandValue.text = rules.MaxHandSize.ToString();
    }

    private void ChangeStocks(int amount)
    {
        MatchRules rules = Rules;
        if (rules == null) return;
        rules.SetStocks(rules.Stocks + amount);
        Refresh();
    }

    private void ChangeRounds(int amount)
    {
        MatchRules rules = Rules;
        if (rules == null) return;
        rules.SetRoundsToWin(rules.RoundsToWin + amount);
        Refresh();
    }

    private void ChangeInitialHand(int amount)
    {
        MatchRules rules = Rules;
        if (rules == null) return;
        rules.SetInitialHandSize(rules.InitialHandSize + amount);
        Refresh();
    }

    private void ChangeMaxHand(int amount)
    {
        MatchRules rules = Rules;
        if (rules == null) return;
        rules.SetMaxHandSize(rules.MaxHandSize + amount);
        Refresh();
    }
}
