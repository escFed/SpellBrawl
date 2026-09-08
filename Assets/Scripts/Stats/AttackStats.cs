using UnityEngine;

public abstract class AttackStats : ScriptableObject
{
    [Header("Frame Data")]
    public float startup = 0.05f;
    public float active = 0.05f;
    public float recovery = 0.15f;

    [Header("Hit Result")]
    public float hitStun = 0.4f;
    public KnockbackProfile launch = new KnockbackProfile();
    public HitReaction hitReaction = HitReaction.Hit;

    [Header("Audio")]
    [Tooltip("Optional impact sound. Leave empty to use the shared CombatAudioSettings hit clip.")]
    public AudioClip hitSound;

    public int energyGain = 10;
}
