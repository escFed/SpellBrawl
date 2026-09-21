using System.Collections.Generic;
using UnityEngine;

public sealed class CharacterAnimationController
{
    private const int BaseLayer = 0;

    private readonly Animator animator;
    private readonly Object logContext;
    private readonly Dictionary<int, AnimatorControllerParameterType> parameterTypes = new();
    private readonly HashSet<string> reportedConfigurationIssues = new();

    public CharacterAnimationController(Animator animator, Object logContext = null)
    {
        this.animator = animator;
        this.logContext = logContext;
        CacheParameters();
    }

    public bool TryPlay(int stateHash)
    {
        if (!CanUseAnimator())
            return false;

        if (!animator.HasState(BaseLayer, stateHash))
        {
            ReportOnce(
                $"state:{stateHash}",
                $"Animator state hash '{stateHash}' is not configured on '{animator.name}'.");
            return false;
        }

        animator.Play(stateHash, BaseLayer, 0f);
        return true;
    }

    public bool TryPlay(string stateName)
    {
        return TryPlayStateName(stateName);
    }

    public bool TryPlay(string stateName, string fallbackStateName)
    {
        return TryPlayStateName(stateName) || TryPlayStateName(fallbackStateName);
    }

    public bool TrySetFloat(int parameterHash, float value)
    {
        if (!HasParameter(parameterHash, AnimatorControllerParameterType.Float))
            return false;

        animator.SetFloat(parameterHash, value);
        return true;
    }

    public bool TrySetInt(int parameterHash, int value)
    {
        if (!HasParameter(parameterHash, AnimatorControllerParameterType.Int))
            return false;

        animator.SetInteger(parameterHash, value);
        return true;
    }

    public bool TrySetBool(int parameterHash, bool value)
    {
        if (!HasParameter(parameterHash, AnimatorControllerParameterType.Bool))
            return false;

        animator.SetBool(parameterHash, value);
        return true;
    }

    public bool TrySetTrigger(int parameterHash)
    {
        if (!HasParameter(parameterHash, AnimatorControllerParameterType.Trigger))
            return false;

        animator.SetTrigger(parameterHash);
        return true;
    }

    public bool TryFitCurrentStateToDuration(int stateHash, int speedParameterHash, float duration)
    {
        if (duration <= 0f || !HasParameter(speedParameterHash, AnimatorControllerParameterType.Float) ||
            !CanUseAnimator() || !animator.isActiveAndEnabled)
            return false;

        animator.Update(0f);
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(BaseLayer);
        if (state.fullPathHash != stateHash)
            return false;

        animator.SetFloat(speedParameterHash, Mathf.Max(1f, state.length / duration));
        return true;
    }

    private void CacheParameters()
    {
        if (animator == null)
            return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
            parameterTypes[parameter.nameHash] = parameter.type;
    }

    private bool HasParameter(int parameterHash, AnimatorControllerParameterType expectedType)
    {
        if (!CanUseAnimator())
            return false;

        if (!parameterTypes.TryGetValue(parameterHash, out AnimatorControllerParameterType actualType))
        {
            ReportOnce(
                $"parameter:{parameterHash}",
                $"Animator parameter hash '{parameterHash}' is not configured on '{animator.name}'.");
            return false;
        }

        if (actualType == expectedType)
            return true;

        ReportOnce(
            $"parameter-type:{parameterHash}:{expectedType}",
            $"Animator parameter hash '{parameterHash}' on '{animator.name}' is {actualType}, expected {expectedType}.");
        return false;
    }

    private bool TryPlayStateName(string stateName)
    {
        return !string.IsNullOrWhiteSpace(stateName) &&
            TryPlay(Animator.StringToHash($"Base Layer.{stateName}"));
    }

    private bool CanUseAnimator()
    {
        if (animator != null && animator.layerCount > BaseLayer)
            return true;

        ReportOnce(
            "animator",
            animator == null
                ? "Character animation request ignored because no Animator is configured."
                : $"Character animation request ignored because '{animator.name}' has no base layer.");
        return false;
    }

    private void ReportOnce(string key, string message)
    {
        if (reportedConfigurationIssues.Add(key))
            Debug.LogWarning($"[CharacterAnimationController] {message}", logContext);
    }
}
