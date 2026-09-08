using UnityEngine;
using UnityEngine.UI;

public class TsunamiCard : MonoBehaviour, ICardable
{

    [Header("Card Info")]
    [SerializeField] private string cardName = "Tsunami";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Invokes a huge wave of water, causing significant damage to all enemies.";
    [SerializeField] private string damageableOrNot = "Damage";

    [Header("Card Settings")]
    [SerializeField] private int energyCost = 30;
    [SerializeField] private CardType type = CardType.OFFENSIVE;
    [SerializeField] private Sprite cardVisual;
    [SerializeField] private Image cardUI;

    [SerializeField] private GameObject tsunamiPrefab;

    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public int EnergyCost => energyCost;
    public CardType Type => type;
    public Sprite CardVisual => cardVisual;
    public string DamageableOrNot => damageableOrNot;



    public void SetUI(Image uiImage)
    {
        cardUI = uiImage;
        if (cardUI != null)
        {
            cardUI.sprite = cardVisual;

            cardUI.enabled = true;
        }
    }

    public bool CanBeUsed(PlayerController user) => true;

    public void ExecuteCard(PlayerController character)
    {
        PlayerController target = null;
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController p in allPlayers)
        {
            if (p.gameObject != character.gameObject)
            {
                target = p;
                break;
            }
        }

        if (target != null)
        {
            int damageAmount = 50;

            if (tsunamiPrefab != null)
            {
                GameObject waveInstance = Instantiate(tsunamiPrefab, character.transform.position, Quaternion.identity);
                TsunamiWave wave = waveInstance.GetComponent<TsunamiWave>();

                if (wave != null)
                {
                    // Inicializamos la ola con caster, target y duración
                    wave.Init(character.gameObject, target.transform, 5f);

                    // Aplicamos daño directo al target
                    target.Health.TakeDamage(damageAmount, Vector2.zero);
                }
                else
                {
                    Debug.LogError("❌ El prefab Tsunami no tiene TsunamiWave adjunto.");
                }
            }
            else
            {
                Debug.LogError("❌ Prefab Tsunami no asignado en TsunamiCard.");
            }

            Debug.Log($"{character.name} usó {cardName} contra {target.name}, causando {damageAmount} de daño!");
        }
        else
        {
            Debug.Log($"{character.name} usó {cardName}, pero no había objetivos válidos.");
        }
    }


}
