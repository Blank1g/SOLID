using UnityEngine;
using UnityEngine.EventSystems;
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

    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    bool isGrounded;
    bool hasControl = true;
    float ySpeed;
    Vector3 desiredMoveDirection;
    Vector3 moveDirection;
    Vector3 velocity;

    Quaternion targetRotation;
    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    EnvironmentScanner environmentScanner;

    public float RotationSpeed => rotationSpeed;

    public void OnEnable()
    {
        moveAction.Enable();
    }

    public void OnDisable()
    {
        moveAction.Disable();
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
    }

    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }

    private void LedgeMovement()
    {
        var signedAngle = Vector3.SignedAngle(
            LedgeData.surfaceHit.normal,
            desiredMoveDirection,
            Vector3.up
        );
        var angle = Mathf.Abs(signedAngle);

        if (Vector3.Angle(desiredMoveDirection, transform.forward) >= 80)
        {
            velocity = Vector3.zero;
            return;
        }

        if (angle < 60)
        {
            velocity = Vector3.zero;
            moveDirection = Vector3.zero;
        }
        else if (angle < 90)
        {
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var direction = left * Mathf.Sign(signedAngle);
            velocity = velocity.magnitude * direction;
            moveDirection = direction;
        }
    }

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        Vector2 mAction = moveAction.ReadValue<Vector2>();
        Vector3 moveInput = new Vector3(mAction.x, 0, mAction.y);
        float moveAmount = Mathf.Clamp(Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.z), 0, 1f);
        var cameraProjection = cameraController.GetCameraProjection();
        desiredMoveDirection =
            cameraProjection.forward * moveInput.z + cameraProjection.right * moveInput.x;
        moveDirection = desiredMoveDirection;

        if (!hasControl)
        {
            return;
        }

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            ySpeed = -0.5f;
            velocity = desiredMoveDirection * moveSpeed;

            IsOnLedge = environmentScanner.LedgeCheck(
                desiredMoveDirection,
                out LedgeData ledgeData
            );

            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;

            velocity = transform.forward * moveSpeed / 2;
        }

        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0 && moveDirection.magnitude > 0.2f)
        {
            targetRotation = Quaternion.LookRotation(
                new Vector3(moveDirection.x, 0, moveDirection.z)
            );
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
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
