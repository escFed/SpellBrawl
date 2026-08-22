using UnityEngine;
using UnityEngine.UI;

public class StarThrowCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private float spawnHeight = 12f;
    public int EnergyCost => 40;

<<<<<<< Updated upstream:Assets/Scripts/Cards/Star/StarThrowCard.cs
    public Sprite cardSprite;

    private Image cardUI;
    private bool canUse = true;
=======
    [Header("Visual")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private Image cardUI;

    public string DamageableOrNot => damageOrNot;
    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Offensive;
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Star/StarThrowCard.cs

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
<<<<<<< Updated upstream:Assets/Scripts/Cards/Star/StarThrowCard.cs
        if (!canUse) return;
        StartCoroutine(StarRoutine(player));
    }

    private IEnumerator StarRoutine(PlayerController player)
    {
        canUse = false;
        if (cardUI != null) cardUI.enabled = false;

=======
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Star/StarThrowCard.cs
        PlayerController target = null;
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            if (p.gameObject != player.gameObject)
            {
                target = p;
                break;
            }
        }

        if (target != null)
        {
            Vector3 spawnPosition = new Vector3(player.transform.position.x, player.transform.position.y + spawnHeight, 0f);

            GameObject starObj = Instantiate(starPrefab, spawnPosition, Quaternion.identity);

            if (starObj.TryGetComponent(out StarProjectile starProjectile))
            {
                starProjectile.Init(player.gameObject, target.transform);
            }
        }

        Destroy(gameObject);
    }
}
