using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThunderStrikeCard : MonoBehaviour, ICardable
{
    [SerializeField] private Image cardUI;

    [Header("Settings")]
    [SerializeField] private GameObject tsPrefab;
    [SerializeField] private float cooldownTime = 8f;

    private bool canUse = true;

    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
    }

    public void ExecuteCard(PlayerController player)
    {
        if (!canUse) return;

        StartCoroutine(ThunderRoutine(player));
    }

    private IEnumerator ThunderRoutine(PlayerController player)
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
            GameObject th = Instantiate(tsPrefab, target.transform.position, Quaternion.identity);

            if (th.TryGetComponent(out ThunderProjectile tProj))
            {
                tProj.Init(player.gameObject);
            }
        }

        yield return new WaitForSeconds(cooldownTime);

        canUse = true;

        if (cardUI != null) cardUI.enabled = true;
    }
}