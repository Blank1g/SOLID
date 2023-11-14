using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {

    private InputActions inputActions;

    private void Awake() {
        inputActions = new InputActions();

        inputActions.Player.Enable();
    }

    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = inputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
