using System;
using UnityEngine;

[Serializable]
public class AIProfile
{
    [Range(0f, 1f)]
    public float aggression = 0.7f;
    [Range(0f, 1f)]
    public float defense = 0.6f;
    [Range(0f, 1f)]
    public float cardUsage = 0.5f;
    [Range(0f, 1f)]
    public float parrySkill = 0.25f;
    [Range(0f, 1f)]
    public float grabSkill = 0.55f;
    [Range(0f, 1f)]
    public float shieldUsage = 0.55f;
    [Range(0f, 1f)]
    public float evadeSkill = 0.6f;
    [Range(0f, 1f)]
    public float dashUsage = 0.55f;
    [Range(0f, 1f)]
    public float heavyAttackUsage = 0.5f;
    [Range(0f, 1f)]
    public float boostUsage = 0.65f;
    [Min(0f)]
    public float recoveryFocus = 1f;
    [Range(0f, 1f)]
    public float mistakeChance = 0.15f;
    [Min(0f)]
    public float randomness = 7f;
    [Header("Decision Pacing")]
    [Min(0.05f)]
    public float reactionTime = 0.35f;
    [Min(0f), Tooltip("Minimum seconds between AI card plays. Zero removes this limit.")]
    public float cardUseInterval = 6f;
    [Min(0.05f), Tooltip("Minimum seconds before reconsidering a movement decision.")]
    public float movementDecisionInterval = 0.55f;
    [Min(0.02f)]
    public float shortHopHoldTime = 0.08f;
    [Min(0.05f)]
    public float heavyChargeTime = 0.65f;
    [Min(0.05f)]
    public float shieldHoldTime = 0.75f;

    public void Sanitize()
    {
        aggression = Mathf.Clamp01(aggression);
        defense = Mathf.Clamp01(defense);
        cardUsage = Mathf.Clamp01(cardUsage);
        parrySkill = Mathf.Clamp01(parrySkill);
        grabSkill = Mathf.Clamp01(grabSkill);
        shieldUsage = Mathf.Clamp01(shieldUsage);
        evadeSkill = Mathf.Clamp01(evadeSkill);
        dashUsage = Mathf.Clamp01(dashUsage);
        heavyAttackUsage = Mathf.Clamp01(heavyAttackUsage);
        boostUsage = Mathf.Clamp01(boostUsage);
        recoveryFocus = Mathf.Max(0f, recoveryFocus);
        mistakeChance = Mathf.Clamp01(mistakeChance);
        randomness = Mathf.Max(0f, randomness);
        reactionTime = Mathf.Max(0.05f, reactionTime);
        cardUseInterval = Mathf.Max(0f, cardUseInterval);
        movementDecisionInterval = Mathf.Max(0.05f, movementDecisionInterval);
        shortHopHoldTime = Mathf.Max(0.02f, shortHopHoldTime);
        heavyChargeTime = Mathf.Max(0.05f, heavyChargeTime);
        shieldHoldTime = Mathf.Max(0.05f, shieldHoldTime);
    }
}
