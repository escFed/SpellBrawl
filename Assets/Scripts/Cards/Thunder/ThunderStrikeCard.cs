using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThunderStrikeCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private GameObject tsPrefab;
    [SerializeField] private float cooldownTime = 8f;
    [SerializeField] private int energyCost = 20;

    public int EnergyCost => energyCost;
    public CardType Type => CardType.Offensive;

    [Header("Visual")]
    [SerializeField] public Sprite cardSprite;
    [SerializeField] private Image cardUI;
    private bool canUse = true;

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
        if (!canUse) return;

        StartCoroutine(ThunderRoutine(character));
    }

    private IEnumerator ThunderRoutine(PlayerController character)
    {
        canUse = false;

        if (cardUI != null) cardUI.enabled = false;

        PlayerController target = null;

        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            if (p.gameObject != character.gameObject)
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
                tProj.Init(character.gameObject);
            }
        }

        yield return new WaitForSeconds(cooldownTime);

        canUse = true;

        if (cardUI != null) cardUI.enabled = true;
    }
}