using UnityEngine;

// >>> Import JU TPS Vehicle System Lib
using JUTPS.VehicleSystem;
// >>> Import JU TPS Input System
using JUTPS.JUInputSystem;

namespace JUTPS.VehicleSystem
{
    // >>> Inherit the Vehicle class to use functions and override methods
    public class CustomVehicleExample : Vehicle
    {
        //Floating Force
        [Header("Custom Vehicle Parameters")]
        public float UpForce = 100;
        //  _________________________________________________________________________
        // | If you want to change the control values by another script
        // | like the AI scripts, the variable below must be disabled
        public bool UseDefaultInputs;
        private void Start()
        {
            SetVehicleCenterOfMass(VehicleEngine.CenterOfMass);
        }
        //  _________________________________________________________________________
        // | VehicleUpdate override function works similar to void Update
        protected override void VehicleUpdate()
        {
            //Ground Check
            GroundCheck.GroundCheck(transform);

            //Set default inputs
            if (UseDefaultInputs)
            {
                SetEngineInputs(
                JUInput.GetAxis(JUInput.Axis.MoveHorizontal),   //Left/Right Input Value
                JUInput.GetAxis(JUInput.Axis.MoveVertical),     //Forward/Backward Input Value
                JUInput.GetButton(JUInput.Buttons.JumpButton)); //Brake Vehicle Input Value
            }
        }

        //  _________________________________________________________________________
        // | VehicleUpdate override function works similar to void FixedUpdate
        // | It's exclusively for physics and movement
        // | it's not recommended to do many checks in this override function.
        protected override void VehiclePhysicsUpdate()
        {
            //Turn Off Vehicle
            if (!IsOn) return;

            //Move Vehicle Forward
            AddForwardAcceleration(_vertical * VehicleEngine.TorqueForce);

            //Rotate Vehicle
            transform.Rotate(0, _horizontal * 130 * Time.deltaTime, 0);

            //Ground Aligment
            if (GroundCheck.IsGrounded == true)
            {
                //Vehicle Floating
                float force = Mathf.Lerp(1, 0, Vector3.Distance(GroundCheck.GroundHit.point, transform.position) / GroundCheck.RaycastDistance);
                rb.AddForceAtPosition(GroundCheck.GroundHit.normal * UpForce * force, GroundCheck.GroundHit.point);

                SimulateGroundAlignment(1);
            }
            else
            {
                Align(Vector3.up, 0.5f);
            }

            //Limit Speed
            LimitVehicleSpeed(GroundCheck.IsGrounded, false);
        }
        private void OnDrawGizmos()
        {
            //  _________________________________________________________________________
            // | Tip: Use vehicleGizmos Class
            VehicleGizmo.DrawVehicleGroundCheck(GroundCheck, transform);
        }
    }

}