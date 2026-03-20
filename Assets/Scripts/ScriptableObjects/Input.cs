using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Input", menuName = "Scriptable Objects/Input")]
public class Input : ScriptableObject
{
   [SerializeField] float baseKnockback;
    [SerializeField] float damage;
   [SerializeField] float angle;
   [SerializeField] float knockbackGrowth;

    internal static bool GetKey(KeyCode p)
    {
        throw new NotImplementedException();
    }
}
