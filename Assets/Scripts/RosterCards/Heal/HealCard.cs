using UnityEngine;
using UnityEngine.UI;

public class HealCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "HealCard";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Reduce damage received";
    [SerializeField] private string damageOrNot = "no";

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual;

    public string DamageableOrNot => damageOrNot;
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

    public bool CanBeUsed(CharacterCoordinator user) => true;

    public void ExecuteCard(CharacterCoordinator character)
    {
        if (character != null && character.Health != null && !character.Health.IsDead)
        {
            character.Health.HealDamage(healAmount);

            HealEffect effect = character.GetComponent<HealEffect>();
            if (effect == null)
                effect = character.gameObject.AddComponent<HealEffect>();
            effect.Play(character.Sprite);
        }
        Destroy(gameObject);
    }
}
