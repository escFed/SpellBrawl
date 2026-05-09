using System.Collections;
using UnityEngine;

public class InverseGravityLogic : MonoBehaviour
{
    [SerializeField] public float duration = 1.5f;

   
    public IEnumerator GravityInversion(PlayerController player, float duration)
    {
        this.duration = duration;
        Physics2D.gravity = new Vector2(0, 9.81f);
        yield return new WaitForSeconds(duration);
        Physics2D.gravity = new Vector2(0, -9.81f);


        var card = GetComponent<InverseGravityCard>();
        if (card != null) card.isInverted = true;
    }
}
