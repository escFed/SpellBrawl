using UnityEngine;

public class ThunderStrikeCard : MonoBehaviour, ICardable
{
    [SerializeField] GameObject cardPrefab;

    public void ExecuteCard()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.StartCoroutine(player.ThunderAnimation());
        }
       
    }
}
