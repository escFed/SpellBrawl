using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject characterSelectPanel;
    public GameObject cardsSelectPanel;
    public GameObject controlsPanel;

    public ControlsTextScript textController;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void CharacterSelect()
    {
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void ShowCardsSelect() 
    {
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(true);
    }

    public void BackToCharacterSelect()
    {
        cardsSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void GoToStage1() 
    {
        SceneManager.LoadScene("Stage1");
    }
    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true); textController.StartTypeWriter();
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }



    public void QuitGame()
    {
        Application.Quit();
    }
}