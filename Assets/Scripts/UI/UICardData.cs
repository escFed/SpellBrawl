using UnityEngine;

[CreateAssetMenu(fileName = "UICardData", menuName = "Scriptable Objects/UICardData")]
public class UICardData : ScriptableObject
{
    public string cardName;
    public GameObject cardPrefab;
    [TextArea] public string description;
    public int damage;
    public float cooldown;
}
