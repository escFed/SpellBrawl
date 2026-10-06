using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonBreathLogic : MonoBehaviour 
{

    
    [SerializeField] private float breathDuration = 2f; // Duration of the breath effect
  

    public IEnumerator BreathCorroutine(CharacterCoordinator character)
    {
        // Instanciar el efecto
        GameObject breathEffect = Instantiate(gameObject, character.transform.position, Quaternion.identity);
        breathEffect.transform.SetParent(character.transform);

      

        float elapsed = 0f;

        while (elapsed < breathDuration)
        {
            // Ejemplo: aplicar knockback cada frame
            CharacterMovement movement = character.GetComponent<CharacterMovement>();
            if (movement != null)
            {
                movement.ApplyKnockback(new Vector2(0, 30), 10);
            }

            elapsed += Time.deltaTime;
            yield return null; // pausa hasta el siguiente frame
        }

        Destroy(breathEffect);
    }



}
