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
    public float jumpSpeedMultiplier = 1f;
    public int maxJumps = 2;
    public float weight = 100f;
    public float maxFallSpeed = -15f;
    public float fastFallSpeed = -25f;

    [Header("Combat Stats")]
    public float tiltThreshold = 0.3f;
    public float parryWindow = 0.2f;
    public float defenseMultiplier = 1f;
    public float knockbackMultiplier = 1.5f;
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

    [Header("Ground Normal Attacks")]
    public GroundAttackStats jabAttack;
    public GroundAttackStats fTiltAttack;
    public GroundAttackStats upTiltAttack;
    public GroundAttackStats dTiltAttack;
    public GroundAttackStats dashAttack;

    [Header("Aerial Normal Attacks")]
    public AerialAttackStats neutralAirAttack;
    public AerialAttackStats forwardAirAttack;
    public AerialAttackStats upAirAttack;
    public AerialAttackStats downAirAttack;

    [Header("Heavy Attacks")]
    public HeavyAttackStats forwardHeavyAttack;
    public HeavyAttackStats upHeavyAttack;
    public HeavyAttackStats downHeavyAttack;

    [Header("Grabs")]
    public GrabStats grabStats;
    public GrabStats pivotGrabStats;
    public GrabStats dashGrabStats;

    [Header("Throws")]
    public ThrowStats forwardThrow;
    public ThrowStats backThrow;
    public ThrowStats upThrow;
    public ThrowStats downThrow;
}
