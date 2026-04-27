using UnityEngine;
using System.Collections.Generic;
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public List<GameObject> characterDeck = new List<GameObject>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }   
}
