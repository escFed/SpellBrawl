using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDeckShuffleLogic : MonoBehaviour
{
    private PlayerController target;
    private GameObject ai;


    // BUG FIX 1: En lugar de Start(), usamos Initialize() para recibir
    // el player desde afuera y arrancar la animación de forma controlada
    public void Initialize(PlayerController player)
    {
        target = player;
        ShuffleDeck(target);
    }
 


    private void ShuffleDeck(PlayerController target)
    {
        if (target.DeckSlots.Length == 0)
            return;

        // Mezcla los slots actuales del enemigo (Fisher-Yates)
        for (int i = target.DeckSlots.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (target.DeckSlots[i], target.DeckSlots[j]) = (target.DeckSlots[j], target.DeckSlots[i]);
          
            StartCoroutine(DeckShuffleAnimation());
        }
    }

    public IEnumerator DeckShuffleAnimation()
    {
        if (target.DeckSlots == null || target.DeckSlots.Length == 0)
            yield break;

        float timeForNewAnim = 0.2f;
        float timeSinceStarted = 0f;

        while (timeSinceStarted <= timeForNewAnim)
        {
            timeSinceStarted += Time.deltaTime;
            target.DeckSlots[0].transform.Rotate(0, 0, 360 * Time.deltaTime / timeForNewAnim);
            yield return null;
        }
    }
}