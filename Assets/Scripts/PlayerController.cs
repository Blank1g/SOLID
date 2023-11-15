using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    private InputActions inputActions;
    private InputAction move;

    private Rigidbody rb;
    private float moveSpeed;
    private float startYScale;

    [SerializeField] private float jumpForse = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float crouchYScale = .5f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private MovementState movementState;

    private Vector3 forceDirection = Vector3.zero;

    private enum MovementState {
        Walking,
        Sprinting,
        Crouching,
        CrochingSprinting,
    }

    private void Awake() {
        inputActions = new InputActions();
        rb = this.GetComponent<Rigidbody>();
    }

    private void Start() {
        startYScale = this.transform.localScale.y;
        moveSpeed = walkSpeed;
    }

    private void OnEnable() {
        inputActions.Player.Jump.started += DoJump;
        inputActions.Player.Sprint.started += SprintToggle;
        inputActions.Player.Sprint.canceled += SprintToggle;
        inputActions.Player.Crouch.started += CrouchToggle;
        inputActions.Player.Crouch.canceled += CrouchToggle;
        move = inputActions.Player.Move;

        inputActions.Player.Enable();
    }

    private void FixedUpdate() {
        StateHandler();
        Move();
        LookAt();
    }

    private void OnDisable() {
        inputActions.Player.Jump.started -= DoJump;
        inputActions.Player.Disable();
    }

    private void StateHandler() {
        if (IsGrounded() && movementState == MovementState.Sprinting) {
            moveSpeed = sprintSpeed;
        } else if (IsGrounded() && movementState == MovementState.Walking) {
            moveSpeed = walkSpeed;
        } else if (IsGrounded() && movementState == MovementState.Crouching) {
            moveSpeed = crouchSpeed;
        } else if (IsGrounded() && movementState == MovementState.CrochingSprinting) {
            moveSpeed = crouchSpeed * 2f;
        }
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

    private void Move() {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera);
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera);

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f) {
            rb.velocity -= Vector3.down * Physics.gravity.y * 2f * Time.deltaTime;
        }

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;
        if (horizontalVelocity.sqrMagnitude > moveSpeed * moveSpeed) {
            rb.velocity = horizontalVelocity.normalized * moveSpeed + Vector3.up * rb.velocity.y;
        }
    }

    private void DoJump(InputAction.CallbackContext context) {
        if (IsGrounded()) {
            forceDirection += Vector3.up * jumpForse;
        }
    }

    private void SprintToggle(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (movementState == MovementState.Walking) {
                movementState = MovementState.Sprinting;
            } else if (movementState == MovementState.Crouching) {
                movementState = MovementState.CrochingSprinting;
            }
        } else {
            if (movementState == MovementState.Sprinting) {
                movementState = MovementState.Walking;
            } else if (movementState == MovementState.CrochingSprinting) {
                movementState = MovementState.Crouching;
            }
        }
    }

    private void CrouchToggle(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (movementState == MovementState.Walking) {
                movementState = MovementState.Crouching;
            } else if (movementState == MovementState.Sprinting) {
                movementState = MovementState.CrochingSprinting;
            }

            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        } else {
            if (movementState == MovementState.Crouching) {
                movementState = MovementState.Walking;
            } else if (movementState == MovementState.CrochingSprinting) {
                movementState = MovementState.Sprinting;
            }

            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
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
