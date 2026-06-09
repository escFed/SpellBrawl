using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CountdownManager : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;

    private void Start()
    {
        StartCoroutine(StartMatchRoutine());
    }

    private IEnumerator StartMatchRoutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "FIGHT!";
        }

        EnablePlayers();

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void EnablePlayers()
    {
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in allPlayers)
        {
            if (player.TryGetComponent(out PlayerInput input))
            {
                input.enabled = true;
            }

            if (player.TryGetComponent(out CharacterAI ai))
            {
                ai.enabled = true;
            }
        }
    }
}
