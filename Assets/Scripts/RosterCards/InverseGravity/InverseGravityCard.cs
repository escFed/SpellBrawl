using UnityEngine;
using UnityEngine.UI;

public class InverseGravityCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Gravity";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Lift a nearby rival and reduce their gravity for a short time.";
    [SerializeField] private string damageOrNot = "no";

    [SerializeField] private Sprite cardIcon;
    public CardType Type => CardType.Utility;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;

    // Campo añadido para referencia del Image de UI
    [SerializeField] private Image cardVisual;
    // Implementación requerida por ICardable: devuelve el Sprite (el icono)
    public Sprite CardVisual => cardIcon;
    // Propiedad adicional para exponer el Image si es necesaria en el resto del código
    public Image CardVisualImage => cardVisual;

    [Header("Effect Settings")]
    [SerializeField, Min(0f)] private float effectDuration = 1.75f;
    [SerializeField, Min(0f)] private float range = 6f;
    [SerializeField, Range(0f, 1f)] private float gravityMultiplier = 0.3f;
    [SerializeField, Min(0f)] private float pulseSpeed = 3f;

    public bool CanBeUsed(CharacterCoordinator user)
    {
        return GetRival(user) != null;
    }

    public void ExecuteCard(CharacterCoordinator character)
    {
        CharacterCoordinator rival = GetRival(character);

        if (rival != null)
        {
            AntiGravityEffect debuff = rival.GetComponent<AntiGravityEffect>();
            if (debuff == null)
                debuff = rival.gameObject.AddComponent<AntiGravityEffect>();
            debuff.Apply(effectDuration, gravityMultiplier, pulseSpeed);
        }

        Destroy(gameObject);
    }

    private CharacterCoordinator GetRival(CharacterCoordinator user)
    {
        if (user == null)
            return null;

        CharacterCoordinator[] allPlayers = FindObjectsByType<CharacterCoordinator>(FindObjectsSortMode.None);
        foreach (CharacterCoordinator p in allPlayers)
        {
            if (p == user || p.Health == null || p.Health.Phase != RespawnPhase.Active ||
                p.Movement == null || Vector2.Distance(user.transform.position, p.transform.position) > range)
                continue;

            Rigidbody2D body = p.GetComponent<Rigidbody2D>();
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
                return p;
        }
        return null;
    }

    public void SetUI(Image img)
    {
        if (img != null && cardIcon != null) img.sprite = cardIcon;
        // Mantener referencia al Image de UI para la propiedad CardVisualImage
        if (img != null) cardVisual = img;
    }
}
