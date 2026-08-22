using UnityEngine;

[CreateAssetMenu(fileName = "DeckRules", menuName = "Cards/Deck Rules")]
public class DeckRules : ScriptableObject
{
    [Header("Deck Building")]
    [SerializeField] private int deckSize = 8;
    [SerializeField] private int copiesPerCard = 1;

    [Header("Combat Hand")]
    [SerializeField] private int initialHandSize = 2;
    [SerializeField] private int maxHandSize = 4;
    [SerializeField] private float drawnCardCooldown = 5f;

    [Header("Full Redraw")]
    [SerializeField] private int fullDraws = 1;
    [SerializeField] private int energyCost = 40;
    [SerializeField] private bool recycleStack = true;

    public int DeckSize => deckSize;
    public int CopiesPerCard => copiesPerCard;
    public int InitialHandSize => initialHandSize;
    public int MaxHandSize => maxHandSize;
    public float DrawnCardCooldown => drawnCardCooldown;
    public int FullDraws => fullDraws;
    public int EnergyCost => energyCost;
    public bool RecycleStack => recycleStack;

    private void OnValidate()
    {
        deckSize = Mathf.Max(1, deckSize);
        copiesPerCard = Mathf.Max(1, copiesPerCard);
        maxHandSize = Mathf.Clamp(maxHandSize, 1, 4);
        initialHandSize = Mathf.Clamp(initialHandSize, 1, maxHandSize);
        drawnCardCooldown = Mathf.Max(0f, drawnCardCooldown);
        fullDraws = Mathf.Max(0, fullDraws);
        energyCost = Mathf.Max(0, energyCost);
    }
}
