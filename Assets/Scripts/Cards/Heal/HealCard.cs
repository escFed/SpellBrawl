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

    public void ExecuteCard(PlayerController player)
    {
        StartCoroutine(HealRoutine(player));
    }

    private IEnumerator HealRoutine(PlayerController player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.HealDamage(healAmount);
        }

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}
