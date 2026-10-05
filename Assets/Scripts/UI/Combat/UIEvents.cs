using System;
using UnityEngine;
using SmoothShakeFree;
public static class UIEvents
{
    public static Action<int, int> OnDamageChanged;
    public static Action<int, int> OnLivesChanged;
    public static Action<int, int, int> OnDeckCountChanged;
    public static Action<int, Sprite> OnIconSet;
    public static Action<int, HandSlotView[]> OnHandChanged;

    public static Action<int, int> OnCardUsed;

    public static Action<ShakeBase> ShakeScreen;
    public static event Action<int> OnCardReward;

    public static void InvokeCardReward(int playerIndex)
    {
        OnCardReward?.Invoke(playerIndex);
    }

}
