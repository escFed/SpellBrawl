using System.Collections;
using UnityEngine;

public class FireBallCard : MonoBehaviour, ICardable
{
    [SerializeField] GameObject cardPrefab;

    public void ExecuteCard()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();

        GameObject newFireBall = Instantiate(cardPrefab, player.throwPoint.position, Quaternion.identity);

        FireBallScript script = newFireBall.GetComponent<FireBallScript>();

        Vector2 direction = player.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        script.Init(direction);

        
    }

   
}
