
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackHoleCard : MonoBehaviour, ICardable
{
    [Header("Card Info")]
    [SerializeField] private string cardName = "Black Hole Card";
    [SerializeField, TextArea(3, 5)] private string cardDescription = "Keeps the opponent sticked with you in a 5-Seconds lapse";

    [SerializeField] private string damageOrNot = "60";

    [Header("Black Hole Settings")]


    [SerializeField] private int damage = 25;
    [SerializeField] private int energyCost = 20;

    [Header("Visual")]

    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite cardIcon;


    public int EnergyCost => energyCost;

    public CardType Type => CardType.UTILITY;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string DamageableOrNot => damageOrNot;


    public void SetUI(Image uiImage)
    {
        if (uiImage != null && cardIcon != null) uiImage.sprite = cardIcon;
    }

    public bool CanBeUsed(PlayerController user)
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != user.PlayerIndex)
            {
                return p.IsGrounded;
            }
        }
        return false;
    }
 

   public void ExecuteCard(PlayerController character)
    {
   

        StartCoroutine(ExecuteHoleLogic(character));




    }



    private IEnumerator ExecuteHoleLogic(PlayerController character) 
    {

        PlayerController opponent = null;
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            if (p.PlayerIndex != character.PlayerIndex)
            {
                opponent = p;
                break;
            }
        }

        if (opponent != null)
        {
            if (prefab != null)
            {

                GameObject instance = Instantiate(prefab, opponent.transform.position, Quaternion.identity);

                BlackHoleLogic logic = instance.GetComponent<BlackHoleLogic>();

                if (logic == null)
                {
                    Debug.LogError("No prefab in BlackHoleLogic");
                }

                if (opponent.TryGetComponent(out CharacterHealth opponentHealth))
                {
                    opponentHealth.TakeDamage(damage, new Vector2(0, 1f));
                }

                opponent.Movement.moveSpeedMultiplier = 0.2f;
                opponent.Combat.attackSpeedMultiplier = 0.2f;
                
                logic.Initialize( opponent);
                StartCoroutine(logic.HoleRoutine());

                yield return new WaitForSeconds(logic.effectDuration);

                if (opponent != null)
                {
                    opponent.Movement.moveSpeedMultiplier = 1f;
                    opponent.Combat.attackSpeedMultiplier = 1f;
                }
                        
            }
        }

        Destroy(gameObject);
    }








}
