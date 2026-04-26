using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotUIManager : MonoBehaviour
{
    [Header("Configuration")]

    public int maxCards = 20;


    [Header("Selection Panel")]

    public GameObject panel;


    [Header("Created Slots")]
    public List<CardSlotUI> createdSlots = new List<CardSlotUI>();


    [Header("UI")]
    public Button uiCardButton;
    private List<UICardData> _selectedCards = new();
    public static List<UICardData> SelectedCards { get; private set; } = new();




    public void Start()
    {
        panel.SetActive(true);

        if (uiCardButton != null)
        {


            uiCardButton.interactable = false;
        }

        foreach(CardSlotUI slot in createdSlots)
        {
            slot.OnSelected += HandleSelected;
        }
    }


    private void HandleSelected(CardSlotUI s)
    {
        if(s.IsSelected)
        {
            if(_selectedCards.Count >= maxCards)
            {
                s.DisableCard();
                return;
            }

            _selectedCards.Add(s.Chosen);
        }
        else
        {
            _selectedCards.Remove(s.Chosen);
        }

        UpdateUI();

    }


    public void Confirm()
    {
        SelectedCards = new List<UICardData>(_selectedCards);
        panel.SetActive(false);
        MatchManager.Instance?.StartMatch();
    }


    private void UpdateUI()
    {
        if(uiCardButton != null)
        {
            uiCardButton.interactable = _selectedCards.Count == maxCards;
        }
    }
}
