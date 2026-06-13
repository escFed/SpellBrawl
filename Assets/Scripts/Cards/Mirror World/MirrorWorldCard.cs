using System;
using UnityEngine;
using UnityEngine.UI;

public class MirrorWorldCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Sabotaje de Mano";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Sabotea la mano del rival";

    [Header("Settings Mirror World")]
    [SerializeField] private int energyCost = 30;

    [Header("Visual")]
    [SerializeField] private GameObject mirrorWorldPrefab;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;

    [SerializeField] private CardType cardType;
    public CardType Type => cardType;
    public string CardName => cardName;
    public string CardDescription => cardDescription;

    public void SetUI(Image uiImage)
    {
       if(uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }
    public bool CanBeUsed(PlayerController user) => true;


    public void ExecuteCard(PlayerController character)
    {
        if (character == null)
        {
            Debug.LogError("❌ Character es null en MirrorWorldCard.ExecuteCard");
            return;
        }

        if (mirrorWorldPrefab == null)
        {
            Debug.LogError("❌ Prefab no asignado en MirrorWorldCard");
            return;
        }

        // Instanciás el prefab en la posición del jugador
        GameObject instance = Instantiate(mirrorWorldPrefab, character.transform.position, Quaternion.identity);

        // Buscás el MirrorWorldLogic en el prefab instanciado
        MirrorWorldLogic logic = instance.GetComponent<MirrorWorldLogic>();

        if (logic == null)
        {
            Debug.LogError("❌ El prefab no tiene MirrorWorldLogic adjunto.");
            return;
        }

        // Iniciás la corrutina en el componente correcto
        StartCoroutine(logic.MirrorWorldActivated());
    }




}
