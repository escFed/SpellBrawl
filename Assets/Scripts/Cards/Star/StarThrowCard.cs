using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarThrowCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private float cooldownTime = 6f;
    [SerializeField] private float spawnHeight = 12f;
    [SerializeField] private int energyCost = 20;

    [Header("Visual")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private Image cardUI;
    
    private bool canUse = true;

    public int EnergyCost => energyCost;
    public CardType Type => CardType.Offensive;

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

    public void ExecuteCard(PlayerController player)
    {
        if (!canUse) return;
        StartCoroutine(StarRoutine(player));
    }

    private IEnumerator StarRoutine(PlayerController player)
    {
        canUse = false;
        if (cardUI != null) cardUI.enabled = false;

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

        yield return new WaitForSeconds(cooldownTime);

        canUse = true;
        if (cardUI != null) cardUI.enabled = true;
    }
}
