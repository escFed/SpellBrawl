using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FragileRealityCard : MonoBehaviour, ICardable
{

    [Header("Visual")]
    [SerializeField] private GameObject fragileRealityPrefab;
    [SerializeField] private Sprite cardIcon;
    public int EnergyCost => 15;

    public CardType Type => CardType.Utility;


    public void SetUI(Image uiImage)
    {
        if(uiImage != null && cardIcon != null)
        {
            uiImage.sprite = cardIcon;
        }
    }
    public bool CanBeUsed(PlayerController user) => true; // No specific conditions for using this card, always returns true.
    
      

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
