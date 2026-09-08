using UnityEngine;

[CreateAssetMenu(fileName = "CombatAudioSettings", menuName = "Spell Brawl/Combat Audio Settings")]
public class CombatAudioSettings : ScriptableObject
{
    [Tooltip("Shared impact sound for normal, aerial and Heavy attacks.")]
    public AudioClip hitClip;

    [Range(0f, 1f)]
    public float hitVolume = 1f;
}
