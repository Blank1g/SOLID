using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.JUInputSystem;
using JUTPS.PhysicsScripts;
using JUTPS.FX;
using JUTPS.VehicleSystem;

namespace JUTPS.ActionScripts
{

    [AddComponentMenu("JU TPS/Third Person System/Actions/Drive Vehicles System")]
    public class DriveVehicles : JUTPSActions.JUTPSAction
    {
        private JUFootstep FootstepSoundsToDisable;
        private AdvancedRagdollController Ragdoller;
        [Header("Settings")]
        public Vehicle VehicleToDrive;
        public bool UseDefaultInputsOnVehicleInteraction = true;
        public bool CheckVehiclesByTrigger = true;
        public bool DriveSelectedVehicleOnStart = false;

        [Header("States")]
        public bool VehicleDrivableNearby;
        public bool IsDriving;

        //Events
        public System.Action onEnterVehicle;
        public System.Action onExitVehicle;

        void Start()
        {
            FootstepSoundsToDisable = GetComponent<JUFootstep>();
            Ragdoller = GetComponent<AdvancedRagdollController>();
            if (DriveSelectedVehicleOnStart && VehicleToDrive != null)
            {
                DriveVehicle(VehicleToDrive, gameObject);
            }
        }

        protected virtual void Update()
        {
            DrivingLocomotion();
            if (Ragdoller != null)
            {
                if (Ragdoller.State == AdvancedRagdollController.RagdollState.Ragdolled && IsDriving)
                {
                    ExitVehicle();
                }

                if (Ragdoller.State == AdvancedRagdollController.RagdollState.Ragdolled) VehicleToDrive = null;
            }

            if (UseDefaultInputsOnVehicleInteraction == false) return;

            if (JUInput.GetButtonDown(JUInput.Buttons.EnterVehicleButton))
            {
                if (IsDriving)
                {
                    ExitVehicle();
                }
                else
                {
                    DriveVehicle();
                }
            }
        }
        protected virtual void DrivingLocomotion()
        {
            if (IsDriving == false || VehicleToDrive == null) return;

            //Physic Changes
            if (rb != null)
            {
                //Disable Gravity
                rb.useGravity = false;
                //Simulate Inert
                rb.velocity = VehicleToDrive.rb.velocity;
            }

            //Set Position
            transform.position = VehicleToDrive.InverseKinematicTargetPositions.PlayerLocation.position;
            transform.rotation = VehicleToDrive.InverseKinematicTargetPositions.PlayerLocation.rotation;
        }

        protected virtual void OnEnterVehicle()
        {
            OnJUTPSCharacterStartDriving();
        }
        protected virtual void OnExitVehicle()
        {
            OnJUTPSCharacterStopDriving();
        }

        public void DriveVehicle()
        {
            if (VehicleToDrive == null) return;

            if (FootstepSoundsToDisable != null) FootstepSoundsToDisable.enabled = false;

            if (IsDriving == false)
            {
                OnEnterVehicle();
            }

            //Turn Off Physics
            TPSCharacter.PhysicalIgnore(VehicleToDrive.gameObject, ignore: true);

            //Turn On Vehicle
            VehicleToDrive.IsOn = true;

            //Start driving
            IsDriving = true;
        }
        public void ExitVehicle()
        {
            if (VehicleToDrive == null) return;

            if (FootstepSoundsToDisable != null) FootstepSoundsToDisable.enabled = true;

            if (IsDriving == true)
            {
                if (TPSCharacter != null)
                {
                    TPSCharacter.Invoke("enableMove", 0.1f);

                    TPSCharacter.transform.position = VehicleToDrive.GetExitPosition();
                }
                else
                {
                    transform.position = VehicleToDrive.GetExitPosition();
                }
                OnExitVehicle();
            }
            //Turn On Physics
            Invoke(nameof(ReenablePhysic), 0.2f);

            //Turn Off Vehicle
            VehicleToDrive.IsOn = false;

            //Stop driving
            IsDriving = false;
        }
        private void ReenablePhysic()
        {
            if (IsDriving) return;
            //Turn On Physics
            TPSCharacter.PhysicalIgnore(VehicleToDrive.gameObject, ignore: false);
        }
        public static void DriveVehicle(Vehicle vehicle, GameObject character)
        {
            if (vehicle == null || character == null) return;

            if (character.TryGetComponent(out DriveVehicles driver))
            {
                driver.VehicleToDrive = vehicle;
                driver.DriveVehicle();
            }
            else
            {
                Debug.LogWarning("The character does not have the ability to drive vehicles, assign the script DriveVehicles.cs in gameobject");
            }
        }
        public static void ExitVehicle(GameObject character)
        {
            if (character == null) return;

            if (character.TryGetComponent(out DriveVehicles drive))
            {
                drive.ExitVehicle();
            }
            else
            {
                Debug.LogWarning("Unable to execute exit vehicle command, as specified gameobject does not have the ability to drive vehicles.");
            }
        }
        protected virtual void OnJUTPSCharacterStartDriving()
        {
            // >>> If is JU TPS Character, do changes above
            if (TPSCharacter == null) { return; }

            //Set Driving Animation
            anim.SetBool(TPSCharacter.AnimatorParameters.Driving, true);

            //Change Some Character States
            TPSCharacter.IsJumping = false;
            TPSCharacter.IsGrounded = false;
            TPSCharacter.VelocityMultiplier = 0;

            //Deselect Item
            TPSCharacter.SwitchToItem(-1);

            //Disable Character Locomotion
            TPSCharacter.DisableLocomotion();

            //Change Character Collider Properties
            coll.isTrigger = true;

            //Disable Default Animator Layers
            for (int i = 1; i < 4; i++) anim.SetLayerWeight(i, 0);

            //Disable All Locomotion Animator Parameters
            anim.SetBool(TPSCharacter.AnimatorParameters.Crouch, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.ItemEquiped, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.FireMode, false);
            anim.SetBool(TPSCharacter.AnimatorParameters.Grounded, true);
            anim.SetBool(TPSCharacter.AnimatorParameters.Running, false);
            anim.SetFloat(TPSCharacter.AnimatorParameters.IdleTurn, 0);
            anim.SetFloat(TPSCharacter.AnimatorParameters.Speed, 0);
        }
        protected virtual void OnJUTPSCharacterStopDriving()
        {
            // >>> If is JU TPS Character, do changes above
            if (TPSCharacter == null) { return; }

            //Set Driving Animation
            anim.SetBool(TPSCharacter.AnimatorParameters.Driving, false);

            //Enable Character Locomotion
            TPSCharacter.EnableMove();
            TPSCharacter.transform.eulerAngles = new Vector3(0, TPSCharacter.transform.eulerAngles.y, 0);

            //Change Character Collider Properties
            coll.isTrigger = false;

            //Change Rigidbody gravity
            rb.useGravity = true;

            //Disable Default Animator Layers
            TPSCharacter.ResetDefaultLayersWeight();
        }


        // >>> Physically check vehicle trigger <<<
        private void OnTriggerEnter(Collider other)
        {
            if (!CheckVehiclesByTrigger) return;

            if (other.transform.tag == "VehicleArea" && IsDriving == false)
            {
                VehicleToDrive = other.GetComponentInParent<Vehicle>();

                if (VehicleToDrive != null)
                {
                    VehicleDrivableNearby = true;
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (!CheckVehiclesByTrigger) return;

            if (other.transform.tag == "VehicleArea" && IsDriving == false)
            {
                VehicleToDrive = null;
                VehicleDrivableNearby = false;
            }
        }
    }

}