using UnityEngine;

public class HitReactionComponent : MonoBehaviour
{
   
    public HitReaction currentReaction;

    public void React(HitReaction reactionType, Vector2 knockback)
    {
        currentReaction = reactionType;

        // Acá podés manejar animaciones, efectos, etc.
        Debug.Log($"Reacción: {reactionType}, Knockback: {knockback}");
    }
}



