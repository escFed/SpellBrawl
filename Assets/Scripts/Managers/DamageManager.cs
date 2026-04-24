using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private static DamageManager instance;

    private int globalDamageReduction;
    private Dictionary<int, Vector2> playerKnockbackReductions = new Dictionary<int, Vector2>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // evita duplicados
        }
    }


    // --- DAÑO GLOBAL ---
    public static int CalculateDamage(int baseDamage)
    {
        return Mathf.Max(0, baseDamage - instance.globalDamageReduction);
    }

    public static int AddGlobalDamageReduction(int amount, float duration)
    {
        instance.StartCoroutine(instance.ApplyDamageReduction(amount, duration));
        return amount;
    }

    private IEnumerator ApplyDamageReduction(int amount, float duration)
    {
        globalDamageReduction += amount;
        yield return new WaitForSeconds(duration);
        globalDamageReduction -= amount;
 
    }

    // --- KNOCKBACK POR JUGADOR ---
    public static Vector2 CalculateKnockback(int playerId, Vector2 baseKnockback)
    {
        if (instance == null) return baseKnockback;

        Vector2 reduction = Vector2.zero;
        if (instance.playerKnockbackReductions.ContainsKey(playerId))
            reduction = instance.playerKnockbackReductions[playerId];

       

        

        return new Vector2(
            Mathf.Max(0, baseKnockback.x - reduction.x),
            Mathf.Max(0, baseKnockback.y - reduction.y)
        );
    }


    public static void AddKnockbackReduction(int playerId, Vector2 reduction, float duration)
    {
        instance.StartCoroutine(instance.ApplyKnockbackReduction(playerId, reduction, duration));
    }

    private IEnumerator ApplyKnockbackReduction(int playerId, Vector2 reduction, float duration)
    {
        if (!playerKnockbackReductions.ContainsKey(playerId))
            playerKnockbackReductions[playerId] = Vector2.zero;

        // Activar reducción
        playerKnockbackReductions[playerId] += reduction;

        yield return new WaitForSeconds(duration);

        // Quitar reducción al terminar
        playerKnockbackReductions[playerId] -= reduction;
    }


    public static void UpdateTargetText(int playerId, string message)
    {


        TextMeshProUGUI targetText = null;

        if (playerId == 1)
        {
            targetText = UIManager.Instance.p1_damageText;
        }

        else if (playerId == 2)
        {
            targetText = UIManager.Instance.p2_damageText;
        }

        if (targetText != null)
        {
            targetText.text = message;
            targetText.color = Color.red;
            UIManager.Instance.StartCoroutine(DelayForChangeColor(targetText, Color.white, 3f));
        }


    }

    private static IEnumerator DelayForChangeColor(TextMeshProUGUI text, Color original, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (text != null)
        {
            text.color = original;

        }

    }
}
