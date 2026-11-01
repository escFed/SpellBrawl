using System.Collections;
using UnityEngine;

public class ConsentPopUp : MonoBehaviour
{
    [SerializeField] private GameObject consentPopUp;
    [SerializeField] private MenuManager menuManager;

    private IEnumerator Start()
    {
        yield return null; // esperar a que MenuManager termine su Start

        if (MetricsConsentManager.Instance == null)
        {
            Debug.LogError("MetricsConsentManager no existe en la escena.");
            consentPopUp.SetActive(false);
            yield break;
        }

        bool hasAnswered = MetricsConsentManager.Instance.Answered();

        consentPopUp.SetActive(!hasAnswered);

        if (!hasAnswered)
        {
            menuManager.mainMenuPanel.SetActive(false);
            StartCoroutine(UIFocus.SelectFirstNextFrame(consentPopUp));
        }
    }

    public void OnAccept()
    {
        MetricsConsentManager.Instance.Accept();
        consentPopUp.SetActive(false);
        menuManager.ShowMainMenu();
    }

    public void OnDecline()
    {
        MetricsConsentManager.Instance.Decline();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}