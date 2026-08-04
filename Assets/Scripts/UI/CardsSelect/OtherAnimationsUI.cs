using TMPro;
using UnityEngine;

public class OtherAnimationsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI selectYourCharacterText;
    private Vector2 finalPos;

    void Awake()
    {
        // Guardar la posición final del texto en la UI
        finalPos = selectYourCharacterText.rectTransform.anchoredPosition;

        
    }


    void Start()
    {
        // Colocar el texto inicialmente fuera de pantalla (arriba)
        selectYourCharacterText.rectTransform.anchoredPosition = new Vector2(finalPos.x, finalPos.y + 600f);


        OnSelectCharacterTextApparition();
    }



    public void OnSelectCharacterTextApparition()
    {
        RectTransform textRectTransform = selectYourCharacterText.rectTransform;

        // Animar desde arriba hacia la posición final con delay
        LeanTween.moveY(textRectTransform, finalPos.y, 0.5f)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setDelay(1f); // delay de 1 segundo antes de empezar
    }
}
