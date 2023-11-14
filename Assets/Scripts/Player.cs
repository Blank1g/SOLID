using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private GameInput gameInput;

    private float horizontal;
    private float vertical;

    private void Update() {
        HandleMovement();
    }


    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 projectedCameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Quaternion rotationToCamera = Quaternion.LookRotation(projectedCameraForward, Vector3.up);

        Vector3 moveDir = rotationToCamera * new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float palyerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, palyerRadius, moveDir, moveDistance);

        if (!canMove) {
            Vector3 moveDirX = rotationToCamera * new Vector3(moveDir.x, 0f, 0f).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, palyerRadius, moveDirX, moveDistance);

            if (canMove) {
                moveDir = moveDirX;
            } else {
                Vector3 moveDirZ = rotationToCamera * new Vector3(0f, 0f, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, palyerRadius, moveDirZ, moveDistance);

                if (canMove) {
                    moveDir = moveDirZ;
                } 
            }
        }

        if (canMove) {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);
    }

    public void OnMoveInput(float horizontal, float vertical) {
        this.vertical = vertical;
        this.horizontal = horizontal;
    }
}
