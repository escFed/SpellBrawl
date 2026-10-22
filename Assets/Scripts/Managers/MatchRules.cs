using System;
using UnityEngine;

[Serializable]
public class MatchRules
{
    public const int MaxStocks = 3; // The current combat HUD has three stock icons.
    public const int MaxHandSlots = 4; // The input and HUD expose four card slots.

    [SerializeField] private int stocks = 3;
    [SerializeField] private int roundsToWin = 2;
    [SerializeField] private int initialHandSize = 2;
    [SerializeField] private int maxHandSize = 4;

    public int Stocks => stocks;
    public int RoundsToWin => roundsToWin;
    public int InitialHandSize => initialHandSize;
    public int MaxHandSize => maxHandSize;

    public void SetStocks(int value) => stocks = Mathf.Clamp(value, 1, MaxStocks);
    public void SetRoundsToWin(int value) => roundsToWin = Mathf.Clamp(value, 1, 9);

    public void SetInitialHandSize(int value)
    {
        initialHandSize = Mathf.Clamp(value, 1, maxHandSize);
    }

    public void SetMaxHandSize(int value)
    {
        maxHandSize = Mathf.Clamp(value, initialHandSize, MaxHandSlots);
    }

    public void Normalize()
    {
        SetStocks(stocks);
        SetRoundsToWin(roundsToWin);
        maxHandSize = Mathf.Clamp(maxHandSize, 1, MaxHandSlots);
        SetInitialHandSize(initialHandSize);
    }
}
