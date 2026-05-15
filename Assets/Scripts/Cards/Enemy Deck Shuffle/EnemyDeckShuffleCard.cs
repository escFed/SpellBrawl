using UnityEngine;
using UnityEngine.UI;

public class EnemyDeckShuffleCard : MonoBehaviour, ICardable
{
    [Header("Settings")]
    private Image cardUi;
    public bool canShuffle = true;
    [SerializeField] private GameObject deckShufflePrefab;
    public void SetUI(Image uiImage)
    {
       cardUi = uiImage;
    }
    public void ExecuteCard(PlayerController player)
    {
        if (!canShuffle) return;


        {

            EnemyDeckShuffleLogic shuffleLogic = Instantiate(deckShufflePrefab).GetComponent<EnemyDeckShuffleLogic>();
            shuffleLogic.Initialize(player);

        }
    }



  
}
