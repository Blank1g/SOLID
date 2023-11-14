using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    private InputActions inputActions;
    private InputAction move;

    private Rigidbody rb;

    [SerializeField] private float movementForse = 1f;
    [SerializeField] private float jumpForse = 5f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private Camera playerCamera;

    private Vector3 forceDirection = Vector3.zero;

    private void Awake() {
        inputActions = new InputActions();
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnEnable() {
        inputActions.Player.Jump.started += DoJump;   
        move = inputActions.Player.Move;

        inputActions.Player.Enable();
    }

    private void FixedUpdate() {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera);
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera);

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f) {
            rb.velocity -= Vector3.down * Physics.gravity.y * 2f * Time.deltaTime;
        }

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed) {
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }

        LookAt();
    }

    private void OnDisable() {
        inputActions.Player.Jump.started -= DoJump;
        inputActions.Player.Disable();
    }

    private void LookAt() {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f) {
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        } else {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraForward(Camera playerCamera) {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;

        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera) {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;

        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext context) {
        if (IsGrounded()) {
            forceDirection += Vector3.up * jumpForse;
        }
    }

    private bool IsGrounded() {
        Ray ray = new Ray(this.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f)) {
            return true;
        } else {
            return false;
        }
    }
}
