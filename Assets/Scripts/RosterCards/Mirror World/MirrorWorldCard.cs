using UnityEngine;
using UnityEngine.UI;

public class MirrorWorldCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "MirrorWorld Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Inverse controls";
    [SerializeField] private string damageOrNot = "no";

    [Header("Settings Mirror World")]
    [SerializeField] private int energyCost = 30;

    [Header("Visual")]
    [SerializeField] private GameObject mirrorWorldPrefab;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual; // <<-- añadido

    

    public int EnergyCost => energyCost;

    public CardType Type => CardType.UTILITY;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;
    public Sprite CardVisual => cardIcon; // <<-- cambiado para coincidir con ICardable

    public void SetUI(Image uiImage)
    {
        cardVisual = uiImage;
        if (uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }

    public bool CanBeUsed(PlayerController user) => true;


    public void ExecuteCard(PlayerController caster)
    {
        if (caster == null || mirrorWorldPrefab == null)
        {
            Debug.LogError("[MirrorWorldCard] Invalid execution data.", this);
            Destroy(gameObject);
            return;
        }

        PlayerController target = FindEnemy(caster);
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        GameObject instance = Instantiate(mirrorWorldPrefab, caster.transform.position, Quaternion.identity);

        MirrorWorldLogic logic = instance.GetComponent<MirrorWorldLogic>();
        if (logic == null)
        {
            Debug.LogError("[MirrorWorldCard] The effect prefab has no MirrorWorldLogic component.", this);
            Destroy(instance);
            Destroy(gameObject);
            return;
        }

        logic.Initialize(caster, target);
        logic.Activate();
        Destroy(gameObject);
    }

    private PlayerController FindEnemy(PlayerController caster)
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in allPlayers)
        {
            if (player != caster && !player.IsDead)
                return player;
        }
        return null;
    }
}
