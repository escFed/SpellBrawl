using UnityEngine;
using UnityEngine.UI;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Shadow Spike";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Marks the rival's ground. Spikes erupt after a warning and slow on hit.";
    [SerializeField] private string damageOrNot = "DAMAGEABLE";

    [Header("Settings Shadow Spike")]
    [SerializeField] private int damage = 8;
    [SerializeField, Range(0f, 1f)] private float slowAmount = 0.7f;
    [SerializeField, Min(0f)] private float duration = 1.25f;
    [SerializeField] private Vector2 knockback = Vector2.up;
    [SerializeField, Min(0f)] private float hitStun = 0.3f;
    [SerializeField] private KnockbackProfile launch = new KnockbackProfile { growth = 0f, hitStunPerSpeed = 0f, directionalInfluence = 0f };

    [Header("Visual")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual;

    public string CardName => cardName;
    public string CardDescription => cardDescription;

    public string DamageableOrNot => damageOrNot;
    public CardType Type => CardType.Offensive;

    public Sprite CardVisual => cardIcon;

    public void SetUI(Image img)
    {
        cardVisual = img;
        if (img != null)
            img.sprite = cardIcon;
    }

    public bool CanBeUsed(CharacterCoordinator user)
    {
        if (user == null || spikePrefab == null)
            return false;

        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != user.PlayerIndex && p.IsGrounded)
                return true;
        }
        return false;
    }

    public void ExecuteCard(CharacterCoordinator character)
    {
        if (character == null || spikePrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex == character.PlayerIndex || !p.IsGrounded)
                continue;

            Collider2D body = p.GetComponent<Collider2D>();
            if (body == null)
                break;

            Vector2 groundPosition = new Vector2(p.transform.position.x, body.bounds.min.y);
            GameObject spike = Instantiate(spikePrefab, groundPosition, Quaternion.identity);
            GetComponent<MatchCardUse>()?.CopyTo(spike);
            if (spike.TryGetComponent(out ShadowProjectile trap))
                trap.Initialize(character, p, groundPosition, damage, slowAmount, duration,
                    knockback, hitStun, launch);
            else
            {
                Debug.LogError("Shadow Spike prefab is missing ShadowProjectile", spike);
                Destroy(spike);
            }
            break;
        }

        Destroy(gameObject);
    }
}
