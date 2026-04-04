using UnityEngine;

public class ThunderStrikeScript : MonoBehaviour
{
    [SerializeField] private float twinkleSpeed;
    [SerializeField] public float timeSinceLastTwinkle = 0f;
    [SerializeField] public float timeForNextTwinkle = 0.004f;
    [SerializeField] public float thunderForce;
    [SerializeField] private GameObject thunderPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Platform"))
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();

            player.Rb.AddForce(new Vector2(0, thunderForce), ForceMode2D.Impulse);

            Instantiate(thunderPrefab, player.throwPoint.position, Quaternion.identity);
        }
    }




}
