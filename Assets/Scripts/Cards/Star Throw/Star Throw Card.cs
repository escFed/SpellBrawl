using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarThrowCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private GameObject sthPrefab;
    [SerializeField] private float cooldownBetweenShots = 0.5f;
    [SerializeField] private float reloadTime = 6f;
    [SerializeField] private int maxShoots = 3;
    private Image cardUI;
    private int currentShoots;
    private bool canShoot = true;



    public void SetUI(Image uiImage)
    {
        cardUI = uiImage; // ← esto faltaba
        if (cardUI != null)
        {
            cardUI.sprite = cardUI.GetComponent<Sprite>();
            cardUI.enabled = true;
        }
    }

    private void Awake()
    {
        currentShoots = maxShoots;
    }


    public void ExecuteCard(PlayerController player)
    {
        if (!canShoot || currentShoots <= 0) return;

        canShoot = false;

        GameObject star = Instantiate(sthPrefab, player.throwPoint.position, Quaternion.identity);

        Vector2 direction = player.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (star.TryGetComponent(out StarThrowProjectile script))
        {
            script.Init(direction, player.gameObject);
        }

        currentShoots--;

        if (currentShoots > 0)
        {
            StartCoroutine(ShootCooldown());
        }
        else
        {
            StartCoroutine(ReloadCard());
        }
    }

    private IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(cooldownBetweenShots);
        canShoot = true;
    }

    private IEnumerator ReloadCard()
    {
        if (cardUI != null) cardUI.enabled = false;

        yield return new WaitForSeconds(reloadTime);
        currentShoots = maxShoots;
        canShoot = true;

        if (cardUI != null) cardUI.enabled = true;
    }
}
