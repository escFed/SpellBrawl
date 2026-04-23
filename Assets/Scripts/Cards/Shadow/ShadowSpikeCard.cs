using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{
    [Header("Settings Shadow Spike")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float slowAmount = 0.4f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private int energyCost = 20;

    [Header("Visual")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;
    public CardType Type => CardType.Utility;

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
    }

    public bool CanBeUsed(PlayerController user)
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != user.PlayerIndex)
            {
                return p.IsGrounded;
            }
        }
        return false;
    }

    public void ExecuteCard(PlayerController player)
    {
        StartCoroutine(SpikeRoutine(player));
    }

    private IEnumerator SpikeRoutine(PlayerController player)
    {
        PlayerController rival = null;
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != player.PlayerIndex)
            {
                rival = p;
                break;
            }
        }

        if (rival != null)
        {
            if (spikePrefab != null)
            {
                Instantiate(spikePrefab, rival.transform.position, Quaternion.identity);
            }

            if (rival.TryGetComponent(out PlayerHealth rivalHealth))
            {
                rivalHealth.TakeDamage(damage, new Vector2(0, 1f));
            }

            rival.moveSpeedMultiplier = slowAmount;
            rival.attackSpeedMultiplier = slowAmount;

            yield return new WaitForSeconds(duration);

            if (rival != null)
            {
                rival.moveSpeedMultiplier = 1f;
                rival.attackSpeedMultiplier = 1f;
            }
        }

        Destroy(gameObject);
    }
}
