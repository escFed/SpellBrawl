using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Haste Settings")]
    public float speedMultiplier = 1.5f;
    public float duration = 5f;
    public int energyCost = 45;

    public Sprite cardIcon;

    public int EnergyCost => energyCost;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }

    public void ExecuteCard(PlayerController player)
    {
        StartCoroutine(HasteRoutine(player));
    }

    private IEnumerator HasteRoutine(PlayerController player)
    {
        player.moveSpeedMultiplier = speedMultiplier;
        player.attackSpeedMultiplier = speedMultiplier;

        player.GetComponent<SpriteRenderer>().color = Color.yellow;

        yield return new WaitForSeconds(duration);

        if (player != null)
        {
            player.moveSpeedMultiplier = 1f;
            player.attackSpeedMultiplier = 1f;
            player.GetComponent<SpriteRenderer>().color = Color.white;
        }

        Destroy(gameObject);
    }
}
