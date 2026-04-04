using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class CardStackScript : MonoBehaviour
{
    private List<GameObject> cardStack = new List<GameObject>();
   [SerializeField] private List<GameObject> initialCards = new List<GameObject>();
    private int amountOfCardStack = 2;
    private int cardStackIndex;
    [SerializeField] private Transform handPanel;


    private void Start()
    {
        LoadInitCards();

    }



    void LoadInitCards()
    {
        foreach (var card in initialCards)
        {
            cardStack.Add(card);
        }

        Debug.Log("Cartas iniciales cargadas: " + cardStack.Count);
    }



    public void Push(GameObject card)
    {
        cardStack.Add(card);

        if (cardStack.Count == amountOfCardStack)
        {
            ShuffleCards();
        }
    }



    public GameObject Pop()
    {

        if(cardStack.Count == 0)
        {
            return null;
        }

        GameObject topCard = cardStack[cardStack.Count - 1];
        cardStack.RemoveAt(cardStack.Count - 1);
        return topCard;

    }


    public void ShuffleCards()
    {
        for(int c = 0; c < cardStack.Count; c++)
        {
                int randomIndex = Random.Range(c, cardStack.Count);
                GameObject temp = cardStack[c];
                cardStack[c] = cardStack[randomIndex];
                cardStack[randomIndex] = temp;
        }
    }

    public void DrawCard(PlayerController player)
    {
        
        GameObject drawn = Pop();
        if (drawn != null)
        {
            GameObject cardInstance = Instantiate(drawn, handPanel);
            cardInstance.transform.localScale = Vector3.one;
            Debug.Log("Instanciando carta: " + drawn.name + " en " + handPanel.name);

            if (player.slotCard == null)
            {
                player.slotCard = GameObject.Find("CardFire");
            }

            if (player.slotCard1 == null)
            {
                player.slotCard1 = GameObject.Find("CardThunder");
            }
        }
    }
     
}
