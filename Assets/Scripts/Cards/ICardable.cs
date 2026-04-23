using UnityEngine.UI;

public interface ICardable
{
    int EnergyCost { get; }
    CardType Type { get; }
    bool CanBeUsed(PlayerController user);

    void ExecuteCard(PlayerController player);
    void SetUI(Image uiImage);
}
