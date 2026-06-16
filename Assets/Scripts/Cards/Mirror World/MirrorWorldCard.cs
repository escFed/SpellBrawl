using System;
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

    public int EnergyCost => energyCost;

    public CardType Type => CardType.Utility;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;
    public void SetUI(Image uiImage)
    {
       if(uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }
    public bool CanBeUsed(PlayerController user) => true;


    public void ExecuteCard(PlayerController caster)
    {
        if (caster == null || mirrorWorldPrefab == null)
        {
            Debug.LogError("❌ Datos inválidos en MirrorWorldCard.ExecuteCard");
            return;
        }

        // Buscar enemigo distinto del caster
        PlayerController target = FindEnemy(caster);
        if (target == null)
        {
            Debug.LogWarning("⚠️ No se encontró enemigo para MirrorWorldCard");
            return;
        }

        GameObject instance = Instantiate(mirrorWorldPrefab, caster.transform.position, Quaternion.identity);

        MirrorWorldLogic logic = instance.GetComponent<MirrorWorldLogic>();
        if (logic == null)
        {
            Debug.LogError("❌ El prefab no tiene MirrorWorldLogic adjunto.");
            return;
        }

        logic.Initialize(caster, target);
        StartCoroutine(logic.MirrorWorldActivated());
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
