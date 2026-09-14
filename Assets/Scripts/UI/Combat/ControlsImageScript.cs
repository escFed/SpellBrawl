using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsImageScript : MonoBehaviour
{

    private bool isVisible = true;
    [SerializeField] private GameObject joystickImage;
    [SerializeField] private GameObject keyboardImage1;
    [SerializeField] private GameObject keyboardImage2;

    [SerializeField] private Image controlsImage;
    [SerializeField] private TextMeshProUGUI joystickText;

    [SerializeField] private TextMeshProUGUI keyboardText;

    [SerializeField] private TextMeshProUGUI pressSpacetext;

    [SerializeField] private TextMeshProUGUI controlsText;



    public void Start()
    {
        // TEMPORAL: Resetear para pruebas
        // PlayerPrefs.DeleteKey("ControlsPanelShown");

        // Verificar si el panel ya fue mostrado antes
        int hasShown = PlayerPrefs.GetInt("ControlsPanelShown", 0);
        Debug.Log("ControlsPanelShown: " + hasShown);

        if (hasShown == 1)
        {
            // Desactivar permanentemente
            Debug.Log("Panel ya fue mostrado antes - desactivando");
            HideAllControls();
        }
        else
        {
            // Primera vez: mostrar el panel
            Debug.Log("Primera vez - mostrando panel");
            ShowAllControls();
        }
    }

    public void Update()
    {
        // Solo detectar Space si el panel está visible
        if (isVisible && Input.GetKeyDown(KeyCode.Space))
        {
            HideAllControls();
            // Guardar que ya fue mostrado
            PlayerPrefs.SetInt("ControlsPanelShown", 1);
            PlayerPrefs.Save();
        }
    }

    private void ShowAllControls()
    {
        isVisible = true;
        joystickImage.SetActive(isVisible);
        keyboardImage1.SetActive(isVisible);
        keyboardImage2.SetActive(isVisible);
        controlsImage.gameObject.SetActive(isVisible);
        joystickText.gameObject.SetActive(isVisible);
        keyboardText.gameObject.SetActive(isVisible);
        pressSpacetext.gameObject.SetActive(isVisible);
        controlsText.gameObject.SetActive(isVisible);
    }

    private void HideAllControls()
    {
        isVisible = false;
        joystickImage.SetActive(isVisible);
        keyboardImage1.SetActive(isVisible);
        keyboardImage2.SetActive(isVisible);
        controlsImage.gameObject.SetActive(isVisible);
        joystickText.gameObject.SetActive(isVisible);
        keyboardText.gameObject.SetActive(isVisible);
        pressSpacetext.gameObject.SetActive(isVisible);
        controlsText.gameObject.SetActive(isVisible);
    }
}


