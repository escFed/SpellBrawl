using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "HealCard";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Reduce damage received";
    [SerializeField] private string damageOrNot = "no";

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual;

    public string DamageableOrNot => damageOrNot;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Boost;
    public Sprite CardVisual => cardIcon;
    public Image CardVisualImage => cardVisual;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null)
        {
            img.sprite = cardIcon;
        }
    }

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        StartCoroutine(HealRoutine(character));
    }

    private IEnumerator HealRoutine(PlayerController character)
    {
        CharacterHealth health = character.GetComponent<CharacterHealth>();

        if (health != null)
        {
            health.HealDamage(healAmount);
        }

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}
