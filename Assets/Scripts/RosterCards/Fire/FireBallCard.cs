using UnityEngine;
using UnityEngine.UI;

public class FireBallCard : MonoBehaviour, ICardable
{
<<<<<<< Updated upstream:Assets/Scripts/Cards/Fire/FireBallCard.cs
    [Header("Settings")]
    [SerializeField] private GameObject fbPrefab;
    [SerializeField] private float cooldownBetweenShots = 0.5f;
    [SerializeField] private float reloadTime = 6f;
    [SerializeField] private int maxShoots = 3;
    public int EnergyCost => 40;

    public Sprite cardSprite;
    private Image cardUI;
    private int currentShoots;
    private bool canShoot = true;

    private void Awake()
    {
        currentShoots = maxShoots;
    }

=======
    [Header("Card Info")]
    [SerializeField] private string cardName = "FireBall Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Throws a fireball";
    [SerializeField] private string damageOrNot = "10";

    [Header("Settings")]
    [SerializeField] private GameObject fbPrefab;
    [SerializeField] private int energyCost = 20;

    [Header("Visual")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private Image cardUI;

    public string DamageableOrNot => damageOrNot;
    public int EnergyCost => energyCost;
    public CardType Type => CardType.Offensive;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Fire/FireBallCard.cs
    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;

        if (cardUI != null)
        {
            cardUI.sprite = cardSprite;

            cardUI.enabled = true;
        }
    }

    public void ExecuteCard(PlayerController player)
    {
<<<<<<< Updated upstream:Assets/Scripts/Cards/Fire/FireBallCard.cs
        if (!canShoot || currentShoots <= 0) return;

        canShoot = false;

        GameObject fireball = Instantiate(fbPrefab, player.throwPoint.position, Quaternion.identity);
=======
        Transform spawnPoint = character.Grab != null && character.Grab.throwPoint != null ? character.Grab.throwPoint : character.transform;
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Fire/FireBallCard.cs

        Vector2 direction = player.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (fireball.TryGetComponent(out FireProjectile script))
        {
            script.Init(direction, player.gameObject);
        }

        Destroy(gameObject);
    }
}
