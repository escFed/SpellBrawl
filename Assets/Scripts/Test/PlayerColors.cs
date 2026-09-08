using UnityEngine;

public static class PlayerColors
{
    public static Color PlayerOne = new Color32(45, 190, 255, 255);

    public static Color PlayerTwo = new Color32(255, 65, 65, 255);

    public static Color Get(int playerIndex)
    {
        return playerIndex switch
        {
            0 => PlayerOne,
            1 => PlayerTwo,
            _ => Color.white
        };
    }
}
