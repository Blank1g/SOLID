using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerShooterController : MonoBehaviour {

    [SerializeField]
    float crosshairMaxDistance = 10f;
    [SerializeField]
    float zoomCameraOnAim = 55f;
    [SerializeField]
    LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField]
    Transform crosshair;
    [SerializeField]
    CinemachineVirtualCamera aimCamera;
    
    public InputAction aimAction;

    private Camera mainCamera;

    private enum State {
        Passive,
        Aiming,
        Shooting
    }

    private State state;

    private void Awake() {
        state = State.Passive;
        crosshair.gameObject.SetActive(false);
    }
    
    public void OnEnable() {
        aimAction.Enable();
    }

    public void OnDisable() {
        aimAction.Disable();
    }

    private void Update() {
        if (aimAction.ReadValue<float>() > 0) {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomCameraOnAim, Time.deltaTime * 10f);
            Cursor.lockState = CursorLockMode.None;
            Aim();
        } else {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f, Time.deltaTime * 10f);
            if (state == State.Aiming) {
                crosshair.gameObject.SetActive(false);
                state = State.Passive;
            }
        }
    }

    private void Aim() {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimColliderLayerMask)) {
            if (state == State.Passive) {
                crosshair.position = hit.point;
                crosshair.gameObject.SetActive(true);
            }
            if (Vector3.Distance(transform.position, hit.point) > crosshairMaxDistance) {
                crosshair.position = Vector3.Lerp(crosshair.position, transform.position + (hit.point - transform.position).normalized * crosshairMaxDistance, Time.deltaTime * 10f);
            } else {
                crosshair.position = Vector3.Lerp(crosshair.position, hit.point, Time.deltaTime * 10f);
            }


            // debug ray from player to hit
            Debug.DrawRay(transform.position, hit.point - transform.position, Color.red);
            
            var direction = crosshair.position - transform.position;
            direction.y = 0;
            transform.forward = direction;
            state = State.Aiming;
        }
    }
}
