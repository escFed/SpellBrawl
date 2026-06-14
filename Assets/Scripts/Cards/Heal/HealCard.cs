using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealCard : MonoBehaviour, ICardable
{

    [Header("Card Info")]
    [SerializeField] private string cardName = "Sabotaje de Mano";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotea la mano del rival";
    [SerializeField] private string cost = "////";
    [SerializeField] private string cardType = "";
    [SerializeField] private string damageOrNot = "////";

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;
    public string DamageableOrNot => damageOrNot;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Utility;

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
