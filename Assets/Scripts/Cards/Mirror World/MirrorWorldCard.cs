using System;
using UnityEngine;
using UnityEngine.UI;

public class MirrorWorldCard : MonoBehaviour, ICardable
{


    [Header("Settings Mirror World")]
    [SerializeField] private int energyCost = 30;

    [Header("Visual")]
    [SerializeField] private GameObject mirrorWorldPrefab;
    [SerializeField] private Sprite cardIcon;

    public int EnergyCost => energyCost;

    [SerializeField] private CardType cardType;
    public CardType Type => cardType;


    public void SetUI(Image uiImage)
    {
       if(uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }
    public bool CanBeUsed(PlayerController user) => true;


    public void ExecuteCard(PlayerController character)
    {
       StartCoroutine(character.GetComponent<MirrorWorldLogic>().MirrorWorldActivated());
    }



}
