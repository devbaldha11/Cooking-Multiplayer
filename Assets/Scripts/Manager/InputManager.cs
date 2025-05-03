using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Manager<InputManager>
{
    public Input inputActions;

    public override void Awake()
    {
        base.Awake();
        inputActions = new Input();
        inputActions.Player.Enable();
    }

    public Vector2 GetPlayerMovementVectorNormalized()
    {
#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
        Vector2 keyboardInputVector2 = inputActions.Player.Movement.ReadValue<Vector2>();
        keyboardInputVector2 = keyboardInputVector2.normalized;
        return keyboardInputVector2;
#endif
#if !UNITY_EDITOR || PLATFORM_ANDROID
        Vector2 joystickInputVector2 = inputActions.Player.JoystickMovement.ReadValue<Vector2>();
        joystickInputVector2 = joystickInputVector2.normalized;
        return joystickInputVector2;
#endif
    }

    public override void OnDestroy()
    {
        inputActions.Player.Disable();
        base.OnDestroy();
    }
}
