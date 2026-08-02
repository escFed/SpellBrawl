using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject controlsPanel;
    public GameObject characterSelectPanel;
    public GameObject cardsSelectPanel;


    [Header("Audio")]
    [SerializeField] private AudioClip aMenuButtonClickClip;
    private AudioSource source;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
        cardsSelectPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void ShowHowToPlay()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void CharacterSelect()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
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
        if (controlsPanel != null) controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }


    public void OnButtonPressed()
    {
        source = GetComponent<AudioSource>();
        source.PlayOneShot(aMenuButtonClickClip);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}