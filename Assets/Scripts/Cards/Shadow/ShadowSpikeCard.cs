using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Sabotaje de Mano";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotea la mano del rival";

    [Header("Settings Shadow Spike")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float slowAmount = 0.4f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private int energyCost = 20;

    [Header("Visual")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
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

    public void ExecuteCard(PlayerController character)
    {
        StartCoroutine(SpikeRoutine(character));
    }

    private IEnumerator SpikeRoutine(PlayerController character)
    {
        PlayerController opponent = null;
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != character.PlayerIndex)
            {
                opponent = p;
                break;
            }
        }

        if (opponent != null)
        {
            if (spikePrefab != null)
            {
                Instantiate(spikePrefab, opponent.transform.position, Quaternion.identity);
            }

            if (opponent.TryGetComponent(out CharacterHealth opponentHealth))
            {
                opponentHealth.TakeDamage(damage, new Vector2(0, 1f));
            }

            opponent.Movement.moveSpeedMultiplier = slowAmount;
            opponent.Combat.attackSpeedMultiplier = slowAmount;

            yield return new WaitForSeconds(duration);

            if (opponent != null)
            {
                opponent.Movement.moveSpeedMultiplier = 1f;
                opponent.Combat.attackSpeedMultiplier = 1f;
            }
        }

        Destroy(gameObject);
    }
}
