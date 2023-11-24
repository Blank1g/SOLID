using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.JUInputSystem;

namespace JUTPS.VehicleSystem
{
    [AddComponentMenu("JU TPS/Vehicle System/Car Controller")]
    public class CarController : Vehicle
    {
        [Header("Wheels")]
        public WheelCollider[] WheelColliders;
        public Transform[] WheelModels;

        [Header("Anti Overturn")]
        public VehicleOverturnCheck OverturnCheck;

        [Header("Settings")]
        public bool UseDefaultInputs = true;

        private void Start()
        {
            CreateSteeringWheelRotationPivot(SteeringWheel);
            SetVehicleCenterOfMass(VehicleEngine.CenterOfMass);
        }
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

            //Anti Overturn
            OverturnCheck.OverturnCheck(transform);
            OverturnCheck.AntiOverturn(transform);

            //Update Wheel Models
            for (int i = 0; i < WheelColliders.Length; i++)
            {
                UpdateWheelModelTransformation(WheelColliders[i], WheelModels[i]);
            }

            //Steering Wheel Rotation
            SteeringWheel.transform.localEulerAngles = SteeringWheelRotation(SteeringWheel, WheelColliders[0], 2).eulerAngles;
        }
        protected override void VehiclePhysicsUpdate()
        {
            //Turn Off Vehicle
            if (!IsOn)
            {
                //Set Wheels torque and brake
                for (int i = 0; i < WheelColliders.Length; i++)
                {
                    WheelBrake(WheelColliders[i]);
                }
                return;
            }
            //Set Wheels torque and brake
            for (int i = 0; i < WheelColliders.Length; i++)
            {
                WheelTorque(WheelColliders[i]);
                WheelBrake(WheelColliders[i]);
            }

            //Get Steer Angle direction
            float SteerAngleDirection = Mathf.Lerp(GetSmoothedHorizontalMovement(), GetSmoothedHorizontalMovement() / 4, GetSmoothedForwardMovement() * GetVehicleCurrentSpeed(0.1f));

            //Set Front Wheels Steer Angle
            WheelSteerAngle(WheelColliders[0], SteerAngleDirection * MaxSteerAngle, MaxSteerAngle);
            WheelSteerAngle(WheelColliders[1], SteerAngleDirection * MaxSteerAngle, MaxSteerAngle);

            //On Air Rotation
            if (GroundCheck.IsGrounded == false) Align(Vector3.up, 0.5f);

            //Limit Speed
            LimitVehicleSpeed(GroundCheck.IsGrounded, false);
        }

        private void OnDrawGizmos()
        {
            VehicleGizmo.DrawVector3Position(CharacterExitingPosition, transform, "Exit Position", Color.green);
            VehicleGizmo.DrawOverturnCheck(OverturnCheck, transform);
            VehicleGizmo.DrawVehicleGroundCheck(GroundCheck, transform);
        }
    }

}
