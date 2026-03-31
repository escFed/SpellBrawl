using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Combat")]
    public float tiltThreshold = 0.3f;
}
