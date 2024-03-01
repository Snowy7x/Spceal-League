using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputListener : MonoBehaviour
{
    public event System.Action<Vector2> OnMoveEvent;
    public event System.Action<Vector2> OnLookEvent;
    public event System.Action<float> OnDriveEvent;
    public event System.Action<float> OnReverseEvent;
    public event System.Action<bool, bool, bool> OnJumpEvent;
    public event System.Action<bool, bool, bool> OnDriftEvent;

    public void ButtonPress(InputAction.CallbackContext context, out bool press, out bool hold, out bool release)
    {
        bool isFirstPress = false;
        bool isRelease = false;
        bool isHold = false;
        switch (context.phase)
        {
            case InputActionPhase.Started:
                isFirstPress = true;
                isHold = true;
                isRelease = false;
                break;
            case InputActionPhase.Performed:
                isHold = true;
                break;
            case InputActionPhase.Canceled:
                isFirstPress = false;
                isHold = false;
                isRelease = true;
                break;
        }

        press = isFirstPress;
        hold = isHold;
        release = isRelease;
    }
    
    public void TriggerPress(InputAction.CallbackContext context, out bool press, out bool hold, out bool release, out float value)
    {
        bool isFirstPress = false;
        bool isRelease = false;
        bool isHold = false;
        switch (context.phase)
        {
            case InputActionPhase.Started:
                isFirstPress = true;
                isHold = true;
                isRelease = false;
                break;
            case InputActionPhase.Performed:
                isHold = true;
                isRelease = false;
                isFirstPress = false;
                break;
            case InputActionPhase.Canceled:
                isFirstPress = false;
                isHold = false;
                isRelease = true;
                break;
        }

        press = isFirstPress;
        hold = isHold;
        release = isRelease;
        value = context.ReadValue<float>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        OnMoveEvent?.Invoke(move);
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 look = context.ReadValue<Vector2>();
        OnLookEvent?.Invoke(look);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        bool press, hold, release;
        ButtonPress(context, out press, out hold, out release);
        OnJumpEvent?.Invoke(press, hold, release);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        bool press, hold, release;
        ButtonPress(context, out press, out hold, out release);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        bool press, hold, release;
        ButtonPress(context, out press, out hold, out release);
    }
    
    public void OnPause(InputAction.CallbackContext context)
    {
        bool press, hold, release;
        ButtonPress(context, out press, out hold, out release);
    }
    
    public void OnDrive(InputAction.CallbackContext context)
    {
        //TriggerPress(context, out bool press, out bool hold, out bool release, out float value);
        OnDriveEvent?.Invoke(context.ReadValue<float>());
    }
    
    public void OnReverse(InputAction.CallbackContext context)
    {
        TriggerPress(context, out bool press, out bool hold, out bool release, out float value);
        OnReverseEvent?.Invoke(value);
    }
    
    public void OnDrift(InputAction.CallbackContext context)
    {
        ButtonPress(context, out bool press, out bool hold, out bool release);
        OnDriftEvent?.Invoke(press, hold, release);
    }

    public void RegisterInputClass(CarInput inputC)
    {
        OnMoveEvent += inputC.SetRollInput;
        OnDriveEvent += inputC.SetForwardInput;
        OnReverseEvent += inputC.SetReverseInput;
        OnJumpEvent += inputC.OnJump;
    }
}