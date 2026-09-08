using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Shadow Spike";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Deals damage and slows the target.";
    [SerializeField] private string damageOrNot = "8";

    [Header("Settings Shadow Spike")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float slowAmount = 0.4f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private int energyCost = 20;
    [SerializeField] private Vector2 knockback = Vector2.up;
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 0f, hitStunPerSpeed = 0f, directionalInfluence = 0f };

    [Header("Visual")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual; // añadido: referencia a la imagen UI

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;

    public string DamageableOrNot => damageOrNot;
    public CardType Type => CardType.Offensive;

    // Implementación requerida por ICardable
    public Sprite CardVisual => cardIcon;

    public void SetUI(Image img)
    {
        cardVisual = img;
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
                opponentHealth.ReceiveHit(new CombatHit(damage, knockback, hitStun, HitReaction.Hit, opponent.transform.position, character.PlayerIndex, launch));
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
