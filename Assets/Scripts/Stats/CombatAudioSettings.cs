using UnityEngine;

[CreateAssetMenu(fileName = "CombatAudioSettings", menuName = "Spell Brawl/Combat Audio Settings")]
public class CombatAudioSettings : ScriptableObject
{
    [Tooltip("Shared impact sound for normal, aerial and Heavy attacks.")]
    public AudioClip hitClip;

    [Range(0f, 1f)]
    public float hitVolume = 1f;

    [Tooltip("Played once for each attack that the shield absorbs.")]
    public AudioClip blockClip;

    [Tooltip("Played when the stock-loss visual appears.")]
    public AudioClip stockLossClip;
}
