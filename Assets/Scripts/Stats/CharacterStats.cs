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
    [Tooltip("Horizontal speed of a dodge")]
    public float dodgeSpeed = 14f;

    [Header("Dash Stats")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.18f;
     public float dashRecovery = 0.12f;
    public float dashCooldown = 3f;
    public float dashAttackSpeed = 12f;
    public float dashGrabSpeed = 9f;
    public float dashGrabSlideDuration = 0.16f;

    [Header("Energy Stats")]
    public int maxEnergy = 100;
    public int startingEnergy = 50;

    [Header("Attacks")]
    public AttackStats jabAttack;
    public AttackStats fTiltAttack;
    public AttackStats upTiltAttack;
    public AttackStats dTiltAttack;
    public AttackStats neutralAirAttack;
    public AttackStats forwardAirAttack;
    public AttackStats upAirAttack;
    public AttackStats downAirAttack;
    [Tooltip("Optional. Falls back to Forward Tilt when empty.")]
    public AttackStats dashAttack;

    [Header("Grabs")]
    public GrabStats grabStats;
    public GrabStats pivotGrabStats;
    [Tooltip("Optional. Falls back to the normal grab when empty.")]
    public GrabStats dashGrabStats;

    [Header("Throws")]
    public ThrowStats forwardThrow;
    public ThrowStats backThrow;
    public ThrowStats upThrow;
    public ThrowStats downThrow;
}
