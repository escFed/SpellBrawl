using UnityEngine;

// Follows one offensive card from the consumed hand slot to its spawned effect.
public sealed class MatchCardUse : MonoBehaviour
{
    private int playerIndex = -1;
    private int useId = -1;

    public void Initialize(int ownerIndex, int cardUseId)
    {
        playerIndex = ownerIndex;
        useId = cardUseId;
    }

    public void CopyTo(GameObject effect)
    {
        if (effect == null || useId < 0) return;
        MatchCardUse tracker = effect.GetComponent<MatchCardUse>();
        if (tracker == null) tracker = effect.AddComponent<MatchCardUse>();
        tracker.Initialize(playerIndex, useId);
    }

    public void MarkHit()
    {
        if (useId > 0)
            GameManager.Instance?.Statistics.RecordCardHit(playerIndex, useId);
    }
}
