using UnityEngine;
using UnityEngine.UI;

public class MirrorWorldCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "MirrorWorld Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Reverse the rival's horizontal controls for 5 seconds.";
    [SerializeField] private string damageOrNot = "no";

    [Header("Settings Mirror World")]

    [Header("Visual")]
    [SerializeField] private GameObject mirrorWorldPrefab;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual; // <<-- añadido

    


    public CardType Type => CardType.Utility;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;
    public Sprite CardVisual => cardIcon; // <<-- cambiado para coincidir con ICardable

    public void SetUI(Image uiImage)
    {
        cardVisual = uiImage;
        if (uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }

    public bool CanBeUsed(CharacterCoordinator user) => FindEnemy(user) != null;


    public void ExecuteCard(CharacterCoordinator caster)
    {
        if (caster == null || mirrorWorldPrefab == null)
        {
            Debug.LogError("[MirrorWorldCard] Invalid execution data.", this);
            Destroy(gameObject);
            return;
        }

        CharacterCoordinator target = FindEnemy(caster);
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        MirrorWorldLogic logic = target.GetComponentInChildren<MirrorWorldLogic>();
        GameObject instance = null;
        if (logic == null)
        {
            instance = Instantiate(mirrorWorldPrefab, target.transform.position, Quaternion.identity, target.transform);
            instance.transform.localPosition = Vector3.zero;
            logic = instance.GetComponent<MirrorWorldLogic>();
        }
        if (logic == null)
        {
            Debug.LogError("[MirrorWorldCard] The effect prefab has no MirrorWorldLogic component.", this);
            if (instance != null)
                Destroy(instance);
            Destroy(gameObject);
            return;
        }

        logic.Initialize(caster, target);
        logic.Activate();
        Destroy(gameObject);
    }

    private CharacterCoordinator FindEnemy(CharacterCoordinator caster)
    {
        if (caster == null)
            return null;

        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);

        foreach (CharacterCoordinator player in allPlayers)
        {
            if (player != caster && player.Health != null && player.Health.Phase == RespawnPhase.Active)
                return player;
        }
        return null;
    }
}
