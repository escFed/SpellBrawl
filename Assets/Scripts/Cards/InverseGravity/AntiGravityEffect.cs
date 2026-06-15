using UnityEngine;
using System.Collections;

public class AntiGravityEffect : MonoBehaviour
{
    private Rigidbody2D rb;
    private float originalGravity;

    public void StartEffect(float duration, float floatGravity)
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            originalGravity = rb.gravityScale;
            rb.gravityScale = floatGravity;

            StartCoroutine(RemoveEffectAfterTime(duration));
        }
        else
        {
            Destroy(this);
        }
    }

    private IEnumerator RemoveEffectAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

       
            //if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            //{
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.gravityScale = originalGravity;
            //}

           
        

        Destroy(this);
    }
}
