using UnityEngine;
using UnityEngine.UI;

public class BlackHoleCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Black Hole Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Creates a short gravity field that slows and pulls the opponent while inside.";
    [SerializeField] private string damageOrNot = "NON DAMAGEABLE";

    [Header("Visual")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Image cardVisual;

    public CardType Type => CardType.Utility;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;
    public Sprite CardVisual => cardIcon;

    public void SetUI(Image uiImage)
    {
        if (uiImage != null)
        {
            cardVisual = uiImage;
            if (cardIcon != null)
                uiImage.sprite = cardIcon;
        }
    }

    public bool CanBeUsed(CharacterCoordinator user)
    {
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.PlayerIndex != user.PlayerIndex)
                return player.IsGrounded;
        }

        return false;
    }

    public void ExecuteCard(CharacterCoordinator character)
    {
        CharacterCoordinator opponent = null;
        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.PlayerIndex == character.PlayerIndex)
                continue;

            opponent = player;
            break;
        }

        if (opponent != null && prefab != null)
        {
            GameObject instance = Instantiate(prefab, opponent.transform.position, Quaternion.identity);
            if (instance.TryGetComponent(out BlackHoleLogic logic))
                logic.Initialize(opponent);
            else
            {
                Debug.LogError("Black Hole prefab is missing BlackHoleLogic", instance);
                Destroy(instance);
            }
        }

        Destroy(gameObject);
    }
}
