using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonBreathLogic : MonoBehaviour 
{

    [SerializeField] private GameObject dragonBreathPrefab;
    [SerializeField] private float breathDuration = 3f; // Duration of the breath effect
    


    private void Start()
    {
        // Ensure the prefab is assigned
        if (dragonBreathPrefab == null)
        {
            Debug.LogError("Dragon breath prefab is not assigned in the inspector.");
        }

       
    }

    public IEnumerator BreathCorroutine(CharacterCoordinator character)
    {
        // Instanciar el efecto
        GameObject breathEffect = Instantiate(dragonBreathPrefab, character.transform.position, Quaternion.identity);
        breathEffect.transform.SetParent(character.transform);

        float elapsed = 0f;

        while (elapsed < breathDuration)
        {
            // Ejemplo: aplicar knockback cada frame
            CharacterMovement movement = character.GetComponent<CharacterMovement>();
            if (movement != null)
            {
                movement.ApplyKnockback(new Vector2(0, 30), -10);
            }

            elapsed += Time.deltaTime;
            yield return null; // pausa hasta el siguiente frame
        }

        Destroy(breathEffect);
    }



}
