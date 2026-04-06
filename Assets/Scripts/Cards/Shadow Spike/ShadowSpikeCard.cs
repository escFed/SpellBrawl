using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UI;
using System.Collections;
using Image = UnityEngine.UI.Image;
using Unity.VisualScripting;

public class ShadowSpikeCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    [SerializeField] private Image cardUI;
    [SerializeField] private GameObject shSpPrefab;
    [SerializeField] private bool canUse = true;
    [SerializeField] private ShadowSpikes shadowSpikes;


    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
    }


    public void ExecuteCard(PlayerController player)
    {
        if (!canUse) return;

        canUse = false;

        ShadowSpikes shadSpike = gameObject.AddComponent<ShadowSpikes>();
        shadowSpikes = shadSpike;
        StartCoroutine(ShadowSpikeEffect());
    }


    public IEnumerator ShadowSpikeEffect()
    {
        if (!shadowSpikes.aShSpPrefab)
        {


            yield return new WaitForSeconds(shadowSpikes.delay);
            StartCoroutine(SpawnSpikes());
            StartCoroutine(UnspawnSpikes());

        }
    }

    private IEnumerator SpawnSpikes()
    {
        foreach (Transform pos in shadowSpikes.spawnPos)
        {
            Instantiate(shadowSpikes.aShSpPrefab, pos.position, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }
    }


    private IEnumerator UnspawnSpikes()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

}

