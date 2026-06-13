using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ControlsTextScript : MonoBehaviour
{


    [SerializeField] public TMP_Text controlsText;
    [SerializeField] private float textAparitionSpeed;
    private string controlsString;

    void Awake()
    {
        controlsString = controlsText.text;


    }

    public void StartTypeWriter()
    {
       
            StartCoroutine(ControlsTextCoroutine());
        
    }

    public IEnumerator ControlsTextCoroutine()
    {

        if (controlsString == null)
        {
            Debug.LogError("No control string in the inspector");
            yield break;

        }
        
            // Mostrar todas las letras de golpe con efecto de aparición
            controlsText.text = controlsString;
            controlsText.maxVisibleCharacters = 0;

            // Revelar todas las letras a la vez
            yield return new WaitForSeconds(textAparitionSpeed);
            controlsText.maxVisibleCharacters = controlsString.Length;
        
    }
}
