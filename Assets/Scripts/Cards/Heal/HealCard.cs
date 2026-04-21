using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealCard : MonoBehaviour, ICardable
{
    [Header("Heal Settings")]
    public int healAmount = 25;

    public int energyCost = 30;
    public Sprite cardIcon;

    public int EnergyCost => energyCost;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null)
        {
            img.sprite = cardIcon;
        }
    }

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
