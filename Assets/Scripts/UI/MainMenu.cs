using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject panelControls;
    public GameObject cardsSelect;

    public void PlayGame()
    {
        if (cardsSelect != null)
        {
            cardsSelect.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowControls()
    {
        if (panelControls != null)
        {
            panelControls.SetActive(true);
        }
    }

    public void HideControls()
    {
        if (panelControls != null)
        {
            panelControls.SetActive(false);
        }
    }
}
