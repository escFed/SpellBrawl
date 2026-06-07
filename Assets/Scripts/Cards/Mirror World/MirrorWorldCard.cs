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
        if (character != null && mirrorWorldPrefab != null)
        {
            GameObject mirrorWorldInstance = Instantiate(mirrorWorldPrefab, character.transform.position, Quaternion.identity);
            MirrorWorldLogic mirrorWorldLogic = mirrorWorldInstance.GetComponent<MirrorWorldLogic>();
            if (mirrorWorldLogic != null)
            {
                mirrorWorldLogic.Initialize(character);
                StartCoroutine(mirrorWorldLogic.MirrorWorldActivated());

            }
        }
    }



}
