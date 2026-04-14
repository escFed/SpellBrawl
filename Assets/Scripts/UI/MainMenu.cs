using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject panelControls;

    public void PlayGame()
    {
        SceneManager.LoadScene("Stage1");
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
