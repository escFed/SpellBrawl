using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FortifyCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "FortifyCard";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Reduce damage taken";
    [SerializeField] private string damageOrNot = "no";

    [Header("Defense Settings")]
    public float damageMultiplier = 0.5f;
    public float duration = 3f;
    public int energyCost = 20;

    [Header("UI Settings")]
    public Sprite cardIcon;

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Utility;
    public string DamageableOrNot => damageOrNot;
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
        StartCoroutine(ShieldRoutine(character));
    }

    private IEnumerator ShieldRoutine(PlayerController character)
    {
        CharacterHealth health = character.GetComponent<CharacterHealth>();

        if (health != null)
        {
            health.activeDefenseMultiplier = damageMultiplier;

            character.GetComponent<SpriteRenderer>().color = Color.blue;

            yield return new WaitForSeconds(duration);

            health.activeDefenseMultiplier = 1f;

            character.GetComponent<SpriteRenderer>().color = Color.white;
        }

        Destroy(gameObject);
    }
}
