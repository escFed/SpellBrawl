using UnityEngine;
using UnityEngine.UI;

public class FireBallCard : MonoBehaviour, ICardable
{
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
    public CardType Type => CardType.OFFENSIVE;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;

        if (cardUI != null)
        {
            cardUI.sprite = cardSprite;

            cardUI.enabled = true;
        }
    }

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        Transform spawnPoint = character.Grab != null && character.Grab.throwPoint != null ? character.Grab.throwPoint : character.transform;

        GameObject fireball = Instantiate(fbPrefab, spawnPoint.position, Quaternion.identity);

        Vector2 direction = character.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (fireball.TryGetComponent(out FireProjectile script))
        {
            script.Init(direction, character.gameObject);
        }

        Destroy(gameObject);
    }
}
