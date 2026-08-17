using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonAnim : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rt;
    private int tweenId = -1;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!rt.gameObject.activeInHierarchy) return;

        LeanTween.cancel(rt);
        tweenId = LeanTween.scale(rt, Vector3.one * 0.9f, 0.1f)
            .setEaseInQuad()
            .setOnComplete(() =>
            {
                LeanTween.scale(rt, Vector3.one, 0.1f).setEaseOutQuad();
            }).id;

        Debug.Log("Click en " + gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!rt.gameObject.activeInHierarchy) return;

        LeanTween.cancel(rt);
        tweenId = LeanTween.scale(rt, Vector3.one * 1.1f, 0.15f).setEaseOutQuad().id;

        Debug.Log("Hover en " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!rt.gameObject.activeInHierarchy) return;

        LeanTween.cancel(rt);
        tweenId = LeanTween.scale(rt, Vector3.one, 0.15f).setEaseOutQuad().id;

        Debug.Log("Exit en " + gameObject.name);
    }
}
