using UnityEngine;

[CreateAssetMenu(fileName = "CombatCameraSettings", menuName = "Spell Brawl/Combat Camera Settings")]
public class CombatCameraSettings : ScriptableObject
{
    [Header("Directional shake")]
    [Tooltip("Impulse displacement for a light launch (3 units/second or less).")]
    [Min(0f)] public float minimumShakeStrength = 0.045f;

    [Tooltip("Impulse displacement for a fast launch (18 units/second or more), before reaction and chain scaling.")]
    [Min(0f)] public float maximumShakeStrength = 0.14f;

    [Tooltip("Maximum displacement before the player's Camera Shake setting is applied.")]
    [Min(0f)] public float shakeStrengthCap = 0.23f;

    [Tooltip("Extra shake strength per visual chain step, up to two steps.")]
    [Min(0f)] public float chainStepBonus = 0.18f;

    [Min(0f)] public float lightShakeDuration = 0.1f;
    [Min(0f)] public float strongShakeDuration = 0.16f;
}
