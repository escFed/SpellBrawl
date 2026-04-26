using System;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    [Header("Data")]

    public UICardData cardSlot;
    public Image overlayImage;
    
    public bool IsSelected { get; private set; }

    public event Action<CardSlotUI> OnSelected;

    private UICardData chosen;


    public UICardData Chosen => chosen;






    public void Toggle()
    {
        IsSelected = !IsSelected;
        overlayImage.enabled = IsSelected;
        OnSelected.Invoke(this);
    }



    public void DisableCard()
    {
        overlayImage.enabled = false;
        IsSelected = false;
    }
}
