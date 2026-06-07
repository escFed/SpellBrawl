using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Sabotaje de Mano";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotea la mano del rival";

    [Header("Haste Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Utility;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
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
