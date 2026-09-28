using UnityEngine;

[CreateAssetMenu(fileName = "GroundAttackStats", menuName = "Character/Attacks/Ground Attack")]
public class GroundAttackStats : NormalAttackStats
{
    [Header("Combo Role")]
    [Tooltip("Starts or refreshes the aerial combo chain after an accepted hit.")]
    public bool startsAerialCombo;
}
