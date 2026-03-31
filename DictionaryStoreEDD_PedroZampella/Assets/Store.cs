using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
    private Dictionary<string, Item> anItem = new Dictionary<string, Item>();
   [SerializeField] private Button button1;
   [SerializeField] private Button button2;
    [SerializeField] private Button button3;
    [SerializeField] private Button button4;
    [SerializeField] private Button button5; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Item knife = new Item("Knife", 10);
        Item shotGun = new Item("Shot Gun", 20);
        Item specialShotGun = new Item("Special Shot Gun", 25);
        Item ultraShotGun = new Item("Ultra Shot Gun", 40);
        Item M100 = new Item("M100", 100);

        anItem.Add("knife", knife);
        anItem.Add("shotgun", shotGun);
        anItem.Add("specialshotgun", specialShotGun);
        anItem.Add("ultrashotgun", ultraShotGun);
        anItem.Add("m100", M100);

        button1.onClick.AddListener(() => OnMouseClick("knife"));
        button2.onClick.AddListener(() => OnMouseClick("shotgun"));
        button3.onClick.AddListener(() => OnMouseClick("specialshotgun"));
        button4.onClick.AddListener(() => OnMouseClick("ultrashotgun"));
        button5.onClick.AddListener(() => OnMouseClick("m100"));

    }



    private void OnMouseClick(string itemKey)
    {
        if (anItem.ContainsKey(itemKey))
        {

            Item i = anItem[itemKey];
            Debug.Log($"Ha adquirido {i.name} por {i.prize}");

        }
    }



}
