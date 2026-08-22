using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Character/Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterStats> Characters;
    public CharacterStats GetCharacter(int index) => Characters[index];
    public int CharacterCount => Characters.Count;
}
