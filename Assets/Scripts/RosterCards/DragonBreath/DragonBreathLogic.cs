using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonBreathLogic : MonoBehaviour 
{
    [SerializeField] private float breathDuration = 0.5f; // Duration of the breath effect
    [SerializeField] private GameObject visualPrefab; // Prefab visual separado si lo necesitas

    [SerializeField] private AudioClip breathClip; // Audio clip for the breath sound


    [SerializeField] private AudioSource s; // Audio source to play the sound
    private GameObject breathVisualInstance;
    private CharacterCoordinator target;
    private int attackerPlayerIndex = -1;

    public void SetTargetAndPrefab(CharacterCoordinator newTarget, GameObject breathEffect, int ownerIndex)
    {
        target = newTarget;
        breathVisualInstance = breathEffect;
        attackerPlayerIndex = ownerIndex;
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
                    movement.ApplyKnockback(new Vector2(breathVisualInstance.transform.position.x - target.transform.position.x, breathVisualInstance.transform.position.y - target.transform.position.y), 0.5f);
                }

                // Aplicar daño
                if (targetHealth != null)
                {
                    int before = targetHealth.currentDamage;
                    targetHealth.TakePummelDamage(3, attackerPlayerIndex); 
                    if (targetHealth.currentDamage > before)
                        GetComponent<MatchCardUse>()?.MarkHit();
                }

                lastDamageTime = elapsed;
                s.GetComponent<AudioSource>().PlayOneShot(breathClip);
               
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destruir el efecto visual cuando termine
        if (breathVisualInstance != null)
            Destroy(breathVisualInstance);
    }
}
