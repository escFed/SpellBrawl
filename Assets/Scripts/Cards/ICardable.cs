using UnityEngine;
using UnityEngine.UI;

public interface ICardable
{
    string CardName { get; }
    string CardDescription { get; }
    int EnergyCost { get; }
    CardType Type { get; }

    Sprite CardVisual { get; }
    string DamageableOrNot { get; }
    bool CanBeUsed(PlayerController user);
    void ExecuteCard(PlayerController character);
    void SetUI(Image uiImage);
}
