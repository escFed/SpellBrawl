using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealCard : MonoBehaviour, ICardable
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;
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
