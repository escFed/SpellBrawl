using UnityEngine;
using UnityEngine.UI;

public class FortifyCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "FortifyCard";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Take no damage for 2 seconds. Hits still launch you.";
    [SerializeField] private string damageOrNot = "no";

    [Header("Defense Settings")]
    public float duration = 2f;

    [Header("UI Settings")]
    public Sprite cardIcon;
    [SerializeField] private Image cardVisualImage; // renombrado para evitar conflicto con la interfaz
    public Image CardVisualImage => cardVisualImage; // acceso al Image usado en la UI

    // Implementación que exige la interfaz: Sprite
    public Sprite CardVisual => cardIcon;

    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Boost;
    public string DamageableOrNot => damageOrNot;
    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null)
        {
            img.sprite = cardIcon;
        }
    }

    public bool CanBeUsed(CharacterCoordinator user) => user != null && user.Health != null && !user.Health.IsDamageImmune;

    public void ExecuteCard(CharacterCoordinator character)
    {
        character.Health?.GrantDamageImmunity(duration);
        if (character.Health != null && character.Health.IsDamageImmune)
        {
            FortifyAura aura = character.GetComponent<FortifyAura>();
            if (aura == null)
                aura = character.gameObject.AddComponent<FortifyAura>();
            aura.Play(character.Health, character.Sprite);
        }
        Destroy(gameObject);
    }

    // Implementación explícita corregida (opcional, puede omitirse si ya hay implementación pública)
    Sprite ICardable.CardVisual => cardIcon;
}
