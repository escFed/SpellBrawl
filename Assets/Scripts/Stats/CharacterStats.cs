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
    public float fastFallSpeed = -25f;

    [Header("Combat Stats")]
    public float tiltThreshold = 0.3f;
    public float parryWindow = 0.2f;
    public float defenseMultiplier = 1f;
    public float knockbackMultiplier = 1.5f;
    [Tooltip("Damage multiplier while shielding (0.4 = 60% reduction)")]
    public float shieldDamageMultiplier = 0.4f;
    [Tooltip("Horizontal speed of a dodge")]
    public float dodgeSpeed = 14f;


    [Header("Energy Stats")]
    public int maxEnergy = 100;
    public int startingEnergy = 50;

    [Header("Attacks")]
    public AttackStats jabAttack;
    public AttackStats fTiltAttack;
    public AttackStats upTiltAttack;
    public AttackStats dTiltAttack;
}
