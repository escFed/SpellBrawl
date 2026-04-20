using UnityEngine.UI;

public interface ICardable
{
    int EnergyCost { get; }
    void ExecuteCard(PlayerController player);
    void SetUI(Image uiImage);
}
