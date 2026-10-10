using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonBreathLogic : MonoBehaviour 
{
    [SerializeField] private float breathDuration = 2f; // Duration of the breath effect
    [SerializeField] private GameObject visualPrefab; // Prefab visual separado si lo necesitas

    private GameObject breathVisualInstance;
    private CharacterCoordinator target;

    public void SetTargetAndPrefab(CharacterCoordinator newTarget, GameObject breathEffect)
    {
        target = newTarget;
        breathVisualInstance = breathEffect;
    }

    public IEnumerator BreathCorroutine(CharacterCoordinator targetChar)
    {
        if (targetChar == null)
        {
            Debug.LogError("Dragon Breath: Target es null!");
            if (breathVisualInstance != null)
                Destroy(breathVisualInstance);
            yield break;
        }

        target = targetChar;

        // DEBUG: Mostrar dónde aparece
        if (breathVisualInstance != null)
        {
            Debug.Log($"🔥 Dragon Breath aparece en: {breathVisualInstance.transform.position}, objetivo en: {target.transform.position}");
        }

        float elapsed = 0f;
        float damageInterval = 0.2f; // Hacer daño cada 0.2 segundos
        float lastDamageTime = 0f;

        CharacterHealth targetHealth = target.GetComponent<CharacterHealth>();

        while (elapsed < breathDuration)
        {
            // Aplicar daño cada X segundos
            if (elapsed - lastDamageTime >= damageInterval)
            {
                // Aplicar knockback
                CharacterMovement movement = target.GetComponent<CharacterMovement>();
                if (movement != null)
                {
                    movement.ApplyKnockback(new Vector2(5, 2), 0.5f);
                }

                // Aplicar daño
                if (targetHealth != null)
                {
                    targetHealth.TakePummelDamage(3); 
                }

                lastDamageTime = elapsed;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destruir el efecto visual cuando termine
        if (breathVisualInstance != null)
            Destroy(breathVisualInstance);
    }
}
