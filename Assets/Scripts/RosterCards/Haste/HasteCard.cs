using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Haste Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Increases attack and movement speed";
    [SerializeField] private string damageOrNot = "no";

    [Header("Haste Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;

    // Nuevo campo para cumplir la interfaz
    [SerializeField] private Image cardVisual;

    public string DamageableOrNot => damageOrNot;
    public int EnergyCost => energyCost;
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

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        StartCoroutine(HasteRoutine(character));
    }

    private IEnumerator HasteRoutine(PlayerController character)
    {
        character.Movement.moveSpeedMultiplier = speedMultiplier;
        character.Combat.attackSpeedMultiplier = speedMultiplier;

        character.GetComponent<SpriteRenderer>().color = Color.yellow;

        yield return new WaitForSeconds(duration);

        if (character != null)
        {
            character.Movement.moveSpeedMultiplier = 1f;
            character.Combat.attackSpeedMultiplier = 1f;
            character.GetComponent<SpriteRenderer>().color = Color.white;
        }

        Destroy(gameObject);
    }
}
