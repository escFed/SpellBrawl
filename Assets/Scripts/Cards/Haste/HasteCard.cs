using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HasteCard : MonoBehaviour, ICardable
{
    [Header("Haste Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;
    public CardType Type => CardType.Utility;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }

    public bool CanBeUsed(PlayerController user) => true;

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
