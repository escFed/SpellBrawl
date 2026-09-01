using UnityEngine;

public static class TeamColors
{
    private static Color PlayerOneColor = new Color32(45, 190, 255, 255);

    private static Color PlayerTwoColor = new Color32(255, 65, 65, 255);

    public static Color Get(PlayerSlot slot)
    {
        return slot switch
        {
            PlayerSlot.PlayerOne => PlayerOneColor,
            PlayerSlot.PlayerTwo => PlayerTwoColor,
            _ => Color.white
        };
    }

    public static Color GetWithAlpha(PlayerSlot slot, float alpha)
    {
        Color color = Get(slot);
        color.a = Mathf.Clamp01(alpha);
        return color;
    }
}
