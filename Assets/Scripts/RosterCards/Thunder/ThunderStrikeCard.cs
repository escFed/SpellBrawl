<<<<<<< Updated upstream:Assets/Scripts/Cards/Thunder/ThunderStrikeCard.cs
using System.Collections;
=======
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Thunder/ThunderStrikeCard.cs
using UnityEngine;
using UnityEngine.UI;

public class ThunderStrikeCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private GameObject tsPrefab;
<<<<<<< Updated upstream:Assets/Scripts/Cards/Thunder/ThunderStrikeCard.cs
    [SerializeField] private float cooldownTime = 8f;
    public int EnergyCost => 40;

    public Sprite cardSprite;

    private Image cardUI;
    private bool canUse = true;
=======
    [SerializeField] private int energyCost = 20;

    public int EnergyCost => energyCost;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => CardType.Offensive;
    public string DamageableOrNot => damageOrNot;
    [Header("Visual")]
    [SerializeField] public Sprite cardSprite;
    [SerializeField] private Image cardUI;
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Thunder/ThunderStrikeCard.cs

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
<<<<<<< Updated upstream:Assets/Scripts/Cards/Thunder/ThunderStrikeCard.cs
        if (!canUse) return;

        StartCoroutine(ThunderRoutine(player));
    }

    private IEnumerator ThunderRoutine(PlayerController player)
    {
        canUse = false;

        if (cardUI != null) cardUI.enabled = false;

=======
>>>>>>> Stashed changes:Assets/Scripts/RosterCards/Thunder/ThunderStrikeCard.cs
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
            GameObject th = Instantiate(tsPrefab, target.transform.position, Quaternion.identity);

            if (th.TryGetComponent(out ThunderProjectile tProj))
            {
                tProj.Init(player.gameObject);
            }
        }

        Destroy(gameObject);
    }
}
