using System.Threading.Tasks;
using TaloGameServices;
using UnityEngine;

public class ConsentPopUp : MonoBehaviour
{

    [SerializeField] private GameObject consentPopUp;
    [SerializeField] private MenuManager menuManager;

    private void Start()
    {
        Debug.Log("ConsentPopUp Start() called.");
        if (MetricsConsentManager.Instance == null)
        {
            Debug.LogError("MetricsConsentManager no existe en la escena.");
            return;
        }

        bool hasAnswered = MetricsConsentManager.Instance.Answered();

        consentPopUp.SetActive(!hasAnswered);

        if(!hasAnswered)
        {
            menuManager.mainMenuPanel.SetActive(false);
        }
    }


    public async void OnAccept()
    {
        MetricsConsentManager.Instance.Accept();
        menuManager.mainMenuPanel.SetActive(true);
        consentPopUp.SetActive(false);
    }



    public void OnDecline()
    {
        MetricsConsentManager.Instance.Decline();
        Application.Quit();
    }



}
