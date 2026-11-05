using UnityEngine;
using UI = UnityEngine.UI;

public class DragonCard : MonoBehaviour, ICardable, IMobileCard
{
    [SerializeField] private string cardName = "Dragon Breath";
    [SerializeField, TextArea] private string cardDescription = "Breathe fire forward. Staying in the flames leads to a final launch.";
    [SerializeField] private CardType type = CardType.Offensive;
    [SerializeField] private Sprite cardVisual;
    [SerializeField] private string damageableOrNot = "12";

    [Header("Visual")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private UI.Image cardUI;
    [SerializeField] private GameObject dragonPrefab;

    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public CardType Type => type;
    public Sprite CardVisual => cardVisual;
    public string DamageableOrNot => damageableOrNot;

    public void SetUI(UI.Image uiImage)
    {
        cardUI = uiImage;
        if (cardUI == null)
            return;

        cardUI.sprite = cardSprite;
        cardUI.enabled = true;
    }

    public bool CanBeUsed(CharacterCoordinator user) => user != null && dragonPrefab != null;

    public void ExecuteCard(CharacterCoordinator character)
    {
        if (!CanBeUsed(character))
            return;

        CharacterHitBox hitBox = character.GetComponent<CharacterHitBox>();
        if (hitBox != null && Mathf.Abs(character.MoveInput.x) > 0.1f)
            hitBox.FaceDirection(character.MoveInput.x);

        float direction = character.Combat.FacingSign;
        GameObject breath = Instantiate(dragonPrefab, character.transform.position, Quaternion.identity);
        GetComponent<MatchCardUse>()?.CopyTo(breath);

        if (breath.TryGetComponent(out DragonBreathLogic logic))
        {
            logic.Initialize(character, direction);
            character.States.Card.SetCard(this, logic.TotalDuration + 0.1f);
        }
        else
        {
            Debug.LogError("Dragon Breath prefab is missing DragonBreathLogic.", dragonPrefab);
            Destroy(breath);
        }

        Destroy(gameObject);
    }
}
