using System;
using UnityEngine;

public static class GameplayAidSettings
{
    private const string LastCardKey = "gameplay.showLastCard";
    private const string AbilityIconsKey = "gameplay.showAbilityIcons";
    private const string InputHistoryKey = "gameplay.showInputHistory";
    // Keep the existing tutorial preference so previous player choices survive.
    private const string TutorialHiddenKey = "MatchTutorialHidden";

    public static event Action Changed;

    public static bool ShowLastCard => PlayerPrefs.GetInt(LastCardKey, 1) != 0;
    public static bool ShowAbilityIcons => PlayerPrefs.GetInt(AbilityIconsKey, 1) != 0;
    public static bool ShowInputHistory => PlayerPrefs.GetInt(InputHistoryKey, 1) != 0;
    public static bool ShowMatchTutorial => PlayerPrefs.GetInt(TutorialHiddenKey, 0) == 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Changed = null;
    }

    public static void SetShowLastCard(bool value)
    {
        Set(LastCardKey, value, ShowLastCard);
    }

    public static void SetShowAbilityIcons(bool value)
    {
        Set(AbilityIconsKey, value, ShowAbilityIcons);
    }

    public static void SetShowInputHistory(bool value)
    {
        Set(InputHistoryKey, value, ShowInputHistory);
    }

    public static void SetShowMatchTutorial(bool value)
    {
        if (ShowMatchTutorial == value)
            return;

        PlayerPrefs.SetInt(TutorialHiddenKey, value ? 0 : 1);
        Changed?.Invoke();
    }

    private static void Set(string key, bool value, bool current)
    {
        if (current == value)
            return;

        PlayerPrefs.SetInt(key, value ? 1 : 0);
        Changed?.Invoke();
    }
}
