using UnityEngine.UI;

public interface ICardable
{
    string CardName { get; }
    string CardDescription { get; }
    int EnergyCost { get; }
    CardType Type { get; }
    bool CanBeUsed(PlayerController user);
    void ExecuteCard(PlayerController character);
    void SetUI(Image uiImage);
}
