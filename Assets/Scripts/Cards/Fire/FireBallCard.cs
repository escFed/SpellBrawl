using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FireBallCard : MonoBehaviour, ICardable
{
    [SerializeField] private Image cardUI;

    [Header("Settings")]
    [SerializeField] private GameObject fbPrefab;
    [SerializeField] private float cooldownBetweenShots = 0.5f;
    [SerializeField] private float reloadTime = 6f;
    [SerializeField] private int maxShoots = 3;

    private int currentShoots;
    private bool canShoot = true;

    private void Awake()
    {
        currentShoots = maxShoots;
    }

    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
    }

    public void ExecuteCard(PlayerController player)
    {
        if (!canShoot || currentShoots <= 0) return;

        canShoot = false;

        GameObject fireball = Instantiate(fbPrefab, player.throwPoint.position, Quaternion.identity);

        Vector2 direction = player.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (fireball.TryGetComponent(out FireProjectile script))
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
