using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System;

public class InputDetector : MonoBehaviour
{
    public static Action<bool> OnGamepadActive;

    private bool isGamepadActive = false;
    private IDisposable inputEventListener;

    private void OnEnable()
    {
        inputEventListener = InputSystem.onAnyButtonPress.Call(ctrl =>
        {
            bool isGamepad = ctrl.device is Gamepad;

            if (isGamepad != isGamepadActive)
            {
                isGamepadActive = isGamepad;
                OnGamepadActive?.Invoke(isGamepad);
            }
        });
    }

    private void OnDisable()
    {
        inputEventListener?.Dispose();
    }
}
