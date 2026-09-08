using System;
using UnityEngine;

[Serializable]
public sealed class KnockbackProfile
{
    [Tooltip("Additional launch speed per 100% damage, independent of base knockback.")]
    [Min(0f)] public float growth = 3f;
    [Tooltip("Seconds of additional stun per unit of final launch speed. Zero keeps stun fixed.")]
    [Min(0f)] public float hitStunPerSpeed = 0.006f;
    [InspectorName("Final Hit Stun Cap"), Min(0f)] public float maxHitStun = 0.8f;
    [Tooltip("Defender angle control for this move. Zero preserves its authored direction.")]
    [Range(0f, 1f)] public float directionalInfluence = 1f;
}

