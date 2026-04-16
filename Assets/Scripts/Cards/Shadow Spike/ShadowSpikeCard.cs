using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{

    [Header("Settings")]
    [SerializeField] private GameObject shadowSpikePrefab;


    public Sprite cardSprite;

    private Image cardUI;
    private bool canUse = true;


    public void SetUI(Image uiImage)
    {
        if (cardUI != null)
        {
            cardUI.sprite = cardSprite;
            cardUI.enabled = true;

        }
    }


    public void ExecuteCard(PlayerController player)
    {
        if (!canUse) return;
        StartCoroutine(ShadowRoutine(player));
    }

    private IEnumerator ShadowRoutine(PlayerController player)
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
            RaycastHit2D hit = Physics2D.Raycast(target.transform.position, Vector2.down, 10f, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                Vector3 spawnPos = new Vector3(target.transform.position.x, hit.point.y, 0f);
                Instantiate(shadowSpikePrefab, spawnPos, Quaternion.identity);
            }

            else
            {
                Instantiate(shadowSpikePrefab, target.transform.position, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(5f);

        canUse = true;
        if (cardUI != null) cardUI.enabled = true;
    }

}
