using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FragileRealityCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "FragileReality Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Creates random platforms";
    [SerializeField] private string damageOrNot = "no";
    public int EnergyCost => 15;

    [SerializeField] private GameObject fragileRealityPrefab;
    public CardType Type => CardType.Utility;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;
    [Header("UI Settings")]
    public Sprite cardIcon;
    public void SetUI(Image uiImage)
    {
        if(uiImage != null && cardIcon != null)
        {
            uiImage.sprite = cardIcon;
        }
    }
    public bool CanBeUsed(PlayerController user) => true; 

    public void ExecuteCard(PlayerController character)
    {
        // Inicia la rutina que instancia y activa la lógica del prefab
        if (character != null)
            StartCoroutine(FragileRealityRoutine(character));
    }
    private IEnumerator FragileRealityRoutine(PlayerController character)
    {
        if (fragileRealityPrefab != null)
        {
            GameObject effect = Instantiate(fragileRealityPrefab);
            FragileRealityLogic logic = effect.GetComponent<FragileRealityLogic>();

            // Buscar un target (ejemplo: el primer otro jugador en la escena)
            PlayerController target = null;
            PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController p in allPlayers)
            {
                if (p != character)
                {
                    target = p;
                    break;
                }
            }

            if (logic != null && target != null)
            {
                logic.Initialize(character, target);
                logic.ActivateFragileReality();
            }
        }

        yield break;
    }


}
