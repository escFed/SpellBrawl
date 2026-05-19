using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Character/CharacterStats")]
public class CharacterStats : ScriptableObject
{
    [Header("Character ID")]
    public string characterName = "New Character";
    public GameObject characterPrefab;
    public Sprite characterIcon;

    [Header("Movement Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public int maxJumps = 2;
    public float weight = 100f;
    public float maxFallSpeed = -15f;

    [Header("Combat Stats")]
    public float tiltThreshold = 0.3f;
    public float parryWindow = 0.2f;
    public float defenseMultiplier = 1f;
    public float knockbackMultiplier = 1.5f;


    [Header("Energy Stats")]
    public int maxEnergy = 100;
    public int startingEnergy = 50;

    [Header("Attacks")]
    public AttackStats jabAttack;
    public AttackStats fTiltAttack;
    public AttackStats upTiltAttack;
    public AttackStats dTiltAttack;
}
