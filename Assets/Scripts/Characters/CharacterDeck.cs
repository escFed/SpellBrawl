using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController), typeof(EnergyManager))]
public class CharacterDeck : MonoBehaviour
{
    private float CardLifetime = 30f;

    [Header("Configuration")]
    [SerializeField] private DeckRules rules;
    [SerializeField] private CardCatalog catalog;

    private PlayerController controller;
    private EnergyManager energy;
    private DeckRuntime runtime;
    private HandSlotView[] handSnapshot;

    public int HandSlotCount => runtime != null ? runtime.HandSlotCount : 0;
    public int EnergyCost => rules != null ? rules.EnergyCost : 0;
    public bool CanFullRedraw => runtime != null && runtime.CanFullRedraw;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergyManager>();

        if (rules == null || catalog == null)
        {
            Debug.LogError("[CharacterDeck] DeckRules and CardCatalog are required.", this);
            enabled = false;
            return;
        }

        handSnapshot = new HandSlotView[rules.MaxHandSize];
    }

    private void Start()
    {
        List<GameObject> deckTemplate = new List<GameObject>(rules.DeckSize);
        IReadOnlyList<GameObject> selectedDeck = GetSelectedPlayerDeck();

        if (!DeckBuilder.TryBuild(selectedDeck, catalog, rules.DeckSize, deckTemplate, out int availableCardCount))
        {
            Debug.LogError($"[CharacterDeck] '{name}' needs {rules.DeckSize} unique valid cards but found {availableCardCount}.", this);
            PublishState();
            return;
        }

        runtime = new DeckRuntime(rules, deckTemplate);
        ResetDeckForNewRound();
    }

    public ICardable GetCardAt(int handIndex)
    {
        return runtime?.GetCardAt(handIndex);
    }

    public bool IsSlotReady(int handIndex)
    {
        return runtime != null && runtime.IsSlotReady(handIndex, Time.time);
    }

    public CardActions TryUseCardFromHand(int handIndex)
    {
        if (runtime == null)
            return CardActions.DeckUnavailable;

        CardActions status = runtime.GetUseStatus(handIndex, Time.time);
        if (status != CardActions.Success)
            return status;

        ICardable cardData = runtime.GetCardAt(handIndex);
        if (!cardData.CanBeUsed(controller))
            return CardActions.CardConditionFailed;
        if (energy.currentEnergy < cardData.EnergyCost)
            return CardActions.NotEnoughEnergy;

        GameObject cardPrefab = runtime.ConsumeAndRefill(handIndex, Time.time + rules.DrawnCardCooldown);

        energy.TrySpendEnergy(cardData.EnergyCost);
        ExecuteCard(cardPrefab);
        PublishState();
        return CardActions.Success;
    }

    public CardActions TryDrawNewHand()
    {
        if (runtime == null || !runtime.CanFullRedraw)
            return CardActions.RedrawUnavailable;
        if (energy.currentEnergy < rules.EnergyCost)
            return CardActions.NotEnoughEnergy;
        if (!runtime.TryFullRedraw(Time.time + rules.DrawnCardCooldown))
            return CardActions.RedrawUnavailable;

        energy.TrySpendEnergy(rules.EnergyCost);
        PublishState();
        return CardActions.Success;
    }

    public void HandleLifeLost()
    {
        if (runtime == null ||
            !runtime.TryUnlockNextSlot(Time.time + rules.DrawnCardCooldown))
        {
            return;
        }

        PublishState();
    }

    public void ForceSabotageRedraw()
    {
        if (runtime != null &&
            runtime.TryForceRedraw(Time.time + rules.DrawnCardCooldown))
        {
            PublishState();
        }
    }

    public void ResetDeckForNewRound()
    {
        if (runtime == null)
            return;

        if (!runtime.ResetForNewRound())
            Debug.LogError("[CharacterDeck] Failed to create the initial hand.", this);

        PublishState();
    }

    private void ExecuteCard(GameObject cardPrefab)
    {
        GameObject cardInstance = Instantiate(cardPrefab, transform.position, Quaternion.identity);
        ICardable card = cardInstance.GetComponent<ICardable>();

        controller.ExecuteCardState(card);
        Destroy(cardInstance, CardLifetime);
    }

    private IReadOnlyList<GameObject> GetSelectedPlayerDeck()
    {
        if (controller.PlayerIndex != 0 || DeckManager.Instance == null)
            return null;

        IReadOnlyList<GameObject> selectedDeck = DeckManager.Instance.SelectedDeck;
        return selectedDeck.Count > 0 ? selectedDeck : null;
    }

    private void PublishState()
    {
        if (runtime != null)
            runtime.CopyHandSnapshotTo(handSnapshot);

        UIEvents.OnHandChanged?.Invoke(controller.PlayerIndex, handSnapshot);
        UIEvents.OnDeckCountChanged?.Invoke(controller.PlayerIndex, runtime != null ? runtime.DrawStackCount : 0, runtime != null ? runtime.FullDraws : 0);
    }
}
