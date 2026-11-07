using UnityEngine;
using UnityEngine.UI;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Haste Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Move 25% faster for 4 seconds. Your next normal attack during Haste recovers 20% faster.";
    [SerializeField] private string damageOrNot = "no";

    [Header("Haste Settings")]
    [SerializeField] private float speedMultiplier = 1.25f;
    [SerializeField] private float duration = 4f;
    [SerializeField, Range(0.1f, 1f)] private float attackRecoveryMultiplier = 0.8f;
    [SerializeField] private Sprite cardIcon;

    // Nuevo campo para cumplir la interfaz
    [SerializeField] private Image cardVisual;

    public string DamageableOrNot => damageOrNot;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Boost;

    // Implementación requerida por ICardable
    public Sprite CardVisual => cardIcon;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
        // Guardar la referencia para la propiedad CardVisual (UI)
        if (img != null) cardVisual = img;
    }

    public bool CanBeUsed(CharacterCoordinator user) => true;

    public void ExecuteCard(CharacterCoordinator character)
    {
        if (character != null)
        {
            HasteEffect effect = character.GetComponent<HasteEffect>();
            if (effect == null)
                effect = character.gameObject.AddComponent<HasteEffect>();
            effect.Apply(duration, speedMultiplier, attackRecoveryMultiplier);
        }
        Destroy(gameObject);
    }
}
