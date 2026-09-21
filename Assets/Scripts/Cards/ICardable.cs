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
    bool CanBeUsed(CharacterCoordinator user);
    void ExecuteCard(CharacterCoordinator character);
    void SetUI(Image uiImage);
}
