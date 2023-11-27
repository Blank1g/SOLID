using JUTPS;
using JUTPS.CameraSystems;
using JUTPS.JUInputSystem;
using JUTPS.UI;
using UnityEngine;

public class DroneCameraController : MonoBehaviour {
    
    public Transform CameraCenterPoint;
    public Transform CrosshairMousePosition;
    private JUCameraController cameraController;
    private JUCharacterController player;

    private void Awake() {
	    SetCameraCenterPoint(); 
	}

    private void Start() {
        cameraController = FindObjectOfType<JUCameraController>();
        var playerobject = GameObject.FindGameObjectWithTag("Player");
        player = playerobject.GetComponent<JUCharacterController>();
        if (player == null) return;
    }

    private void Update() {
        if (player == null) return;
        Debug.Log("Update " + Crosshair.Instance);
        if (Crosshair.Instance.HideOnNoWeaponUsing) {
            HideCrosshairOnNoWeaponUsing();
        }
    }

    private void HideCrosshairOnNoWeaponUsing() {
        CrosshairMousePosition.gameObject.SetActive((player.HoldableItemInUseRightHand || player.HoldableItemInUseLeftHand) ? true : false);
        GetObjectOnCrosshairPoint(cameraController.mCamera, cameraController.CrosshairRaycastLayerMask);
    }
    
    private void GetObjectOnCrosshairPoint(Camera camera, LayerMask CrosshairRaycastLayerMask) {
        Ray MouseRay = camera.ScreenPointToRay(JUInput.GetMousePosition());
        RaycastHit hit;
        if (Physics.Raycast(MouseRay, out hit, 1000, CrosshairRaycastLayerMask))
        {
            CrosshairMousePosition.position = hit.point;
        }
    }

    private void SetCameraCenterPoint() {
		Debug.Log("SetCameraCenterPoint");
		Ray ray = new Ray(transform.position, transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
			CameraCenterPoint.position = hit.point;
		}
	}
}
