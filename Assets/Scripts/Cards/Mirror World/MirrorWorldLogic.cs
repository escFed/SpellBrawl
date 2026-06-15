using System.Collections;
using UnityEngine;

public class MirrorWorldLogic : MonoBehaviour
{
    [SerializeField] private float effectDuration;
    private PlayerController playerController;
    private PlayerController targetController;


    public void Initialize(PlayerController ctrl, PlayerController target)
    {
        playerController = ctrl;

        targetController = target;
    }


        public IEnumerator MirrorWorldActivated()
        {
            if (targetController == null)
            {
                Debug.LogError("❌ Target es null en MirrorWorldLogic");
                yield break;
            }

            Debug.Log($"MirrorWorld afecta al jugador {targetController.PlayerIndex}, lanzado por {playerController.PlayerIndex}");

           

            yield return new WaitForSeconds(3f);


        }
    }


