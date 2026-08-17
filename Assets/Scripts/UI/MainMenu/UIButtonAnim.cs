using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonAnim : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
   
    public void OnPointerClick(PointerEventData eventData)
    {
        Button clickedButton = eventData.pointerPress?.GetComponent<Button>();
        if (clickedButton != null)
        {
            RectTransform rt = clickedButton.GetComponent<RectTransform>();

            LeanTween.cancel(rt);
            LeanTween.scale(rt, Vector3.one * 0.9f, 0.1f)
                     .setEaseInQuad()
                     .setOnComplete(() =>
                     {
                         LeanTween.scale(rt, Vector3.one, 0.1f).setEaseOutQuad();
                     });

           
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button hoveredButton = eventData.pointerEnter?.GetComponent<Button>();
        if (hoveredButton != null)
        {

            RectTransform rt = hoveredButton.GetComponent<RectTransform>();

            // Animar el clicker (aparece con un pequeño zoom)
            LeanTween.cancel(rt);
            LeanTween.scale(rt, Vector3.one * 1.1f, 0.15f).setEaseOutQuad();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Button exitedButton = eventData.pointerEnter?.GetComponent<Button>();
        if (exitedButton != null)
        {
            RectTransform rt = exitedButton.GetComponent<RectTransform>();
            LeanTween.cancel(rt);
            LeanTween.scale(rt, Vector3.one, 0.15f).setEaseOutQuad();
        }

    }
}
