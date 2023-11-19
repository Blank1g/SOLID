using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 5f;

    [SerializeField]
    float rotationSpeed = 500f;

    [Header("Ground Check Settings")]
    [SerializeField]
    float groundCheckRadius = 0.2f;

    [SerializeField]
    Vector3 groundCheckOffset;

    [SerializeField]
    LayerMask groundLayer;

    public InputAction moveAction;
    public InputAction jumpAction;

    bool isGrounded;
    float ySpeed;

    Quaternion targetRotation;
    CameraController cameraController;
    Animator animator;
    CharacterController characterController;

    public void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    public void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 mAction = moveAction.ReadValue<Vector2>();
        Vector3 moveInput = new Vector3(mAction.x, 0, mAction.y);
        float moveAmount = Mathf.Clamp(Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.z), 0, 1f);
        var cameraProjection = cameraController.GetCameraProjection();
        var moveDirection =
            cameraProjection.forward * moveInput.z + cameraProjection.right * moveInput.x;

        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDirection * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDirection);
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        animator.SetFloat("moveAmount", moveAmount, 0.1f, Time.deltaTime);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
