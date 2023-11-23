using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JUTPS.UI
{
    public class UIInteractMessages : MonoBehaviour
    {
        [Header("Item Pickup Message")]
        [SerializeField] private GameObject PickUpMessageObject;
        [SerializeField] private bool SetMessagePositionToItemPosition = true;
        [SerializeField] private Vector3 Offset;
        [SerializeField] private bool ShowItemNameOnText;
        [SerializeField] private Text WarningText;
        [SerializeField] private string PickUpLabelText = "[HOLD] TO PICK UP ";
        [Header("Vehicle Enter Message")]
        [SerializeField] private string VehicleEnterLabelText = "TO DRIVE";
        [SerializeField] private Vector3 VehicleOffset;


        void Update()
        {
            if (JUGameManager.InstancedPlayer == null) { PickUpMessageObject.SetActive(false); return; }

            if (JUGameManager.InstancedPlayer.Inventory == null)
            {
                PickUpMessageObject.SetActive(false);
                gameObject.SetActive(false);
                return;
            }

            // >> Vehicle Message
            if(JUGameManager.InstancedPlayer.VehicleInArea != null && JUGameManager.InstancedPlayer.IsDriving == false)
            {
                PickUpMessageObject.SetActive(true);
                UIElementToWorldPosition.SetUIWorldPosition(PickUpMessageObject, JUGameManager.InstancedPlayer.VehicleInArea.transform.position, VehicleOffset);
                if (WarningText)
                {
                    WarningText.text = VehicleEnterLabelText;
                }
                return;
            }

            // >> Item Message
            PickUpMessageObject.SetActive(JUGameManager.InstancedPlayer.Inventory.ItemToPickUp != null);

            if (PickUpMessageObject.activeInHierarchy && SetMessagePositionToItemPosition)
            {
                UIElementToWorldPosition.SetUIWorldPosition(PickUpMessageObject, JUGameManager.InstancedPlayer.Inventory.ItemToPickUp.transform.position, Offset);
            }

            if(ShowItemNameOnText && WarningText && JUGameManager.InstancedPlayer.Inventory.ItemToPickUp != null)
            {
                WarningText.text = PickUpLabelText + JUGameManager.InstancedPlayer.Inventory.ItemToPickUp.ItemName;
            }
        }
    }
}
