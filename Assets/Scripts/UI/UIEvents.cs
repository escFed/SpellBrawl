using System;
using UnityEngine;

public static class UIEvents
{
    public static Action<int, int> OnDamageChanged;
    public static Action<int, int> OnLivesChanged;
    public static Action<int, int> OnEnergyChanged;
    public static Action<int, int> OnDeckCountChanged;
    public static Action<int, Sprite> OnIconSet;
    public static Action<int, ICardable[]> OnHandChanged;
}
