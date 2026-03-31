using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
public class StoreScript : MonoBehaviour
{
    [SerializeField] private Dictionary<string, ItemScript> anItem = new Dictionary<string, ItemScript>();
    [SerializeField] public Button button1;
    [SerializeField] public Button button2;
    [SerializeField] public Button button3;
    [SerializeField] public Button button4;
    [SerializeField] public Button button5;

    ItemScript shotGun = new ItemScript("ShotGun", 20);
    ItemScript knifeGun = new ItemScript("KnifeGun", 5);
    ItemScript M50Gun = new ItemScript("M50Gun", 45);
    ItemScript subRifleGun = new ItemScript("SubRifleGun", 30);
    ItemScript legendaryRifle = new ItemScript("LegendaryRifle", 200);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shotGun = new ItemScript("ShotGun", 20);
        knifeGun = new ItemScript("KnifeGun", 5);
        M50Gun = new ItemScript("M50Gun", 45);
        subRifleGun = new ItemScript("SubRifleGun", 30);
        legendaryRifle = new ItemScript("LegendaryRifle", 200);


        anItem.Add("knife", knifeGun);
        anItem.Add("shotgun", shotGun);
        anItem.Add("subRifle", subRifleGun);
        anItem.Add("M50", M50Gun);
        anItem.Add("Legendary Rifle", legendaryRifle);

        button1.onClick.AddListener(() => OnMouseClick("knife"));
        button2.onClick.AddListener(() => OnMouseClick("shotgun"));
        button3.onClick.AddListener(() => OnMouseClick("subRifle"));
        button4.onClick.AddListener(() => OnMouseClick("M50"));
        button5.onClick.AddListener(() => OnMouseClick("Legendary Rifle"));



       

        
    }



    private void OnMouseClick(string itemKey)
    {
        if(anItem.ContainsKey(itemKey))
        {
            ItemScript item = anItem[itemKey];
            Debug.Log($"Compraste el siguiente arma {item.itemName} por {item.value}");
        }
    }
        

}





