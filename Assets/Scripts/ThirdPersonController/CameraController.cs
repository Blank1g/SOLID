using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed = 0.1f;

    [SerializeField]
    float minVerticalAngle = -45f;

    [SerializeField]
    float maxVerticalAngle = 45f;

    [SerializeField]
    bool invertX;

    [SerializeField]
    bool invertY;

    [SerializeField]
    Transform cameraCenterPoint;

    public InputAction rotationAction;

    Vector2 rotationInput;

    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    private void Awake() {
        SetCameraCenterPoint();
    }

    public void OnEnable()
    {
        // rotationAction.Enable();
    }

    public void OnDisable()
    {
        // rotationAction.Disable();
    }

    public Transform GetCameraProjection()
    {
        var forward = transform.forward;
        var right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return transform;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (rotationAction.enabled)
        {
            MoveCamera();
        }
    }

    private void MoveCamera()
    {
        rotationInput = rotationAction.ReadValue<Vector2>();

        invertXVal = invertX ? -1f : 1f;
        invertYVal = invertY ? -1f : 1f;

        rotationX += rotationInput.y * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += rotationInput.x * invertXVal * rotationSpeed;

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    private void SetCameraCenterPoint() {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
            cameraCenterPoint.position = hit.point;
        }
    }
}
