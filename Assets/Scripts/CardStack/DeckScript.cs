using Unity.VisualScripting;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    [SerializeField] private CardStackScript cardStack;
    [SerializeField] private PlayerController player;

    private void Start()
    {
        
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }
    }
    private void OnMouseDown()
    {
        cardStack.DrawCard(player);
    }
}
