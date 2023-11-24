using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.JUInputSystem;

namespace JUTPS.VehicleSystem
{

    [AddComponentMenu("JU TPS/Vehicle System/Motorcycle Controller")]
    public class MotorcycleController : Vehicle
    {
        [HideInInspector] public float InclinationValue;

        [Header("Physic Settings")]
        [Range(0, 60)] public float MaxLeanAngle = 45;
        public WheelCollider FrontWheel;
        public WheelCollider BackWheel;
        public Transform FrontWheelModel;
        public Transform BackWheelModel;

        [Header("Anti Overturn")]
        public VehicleOverturnCheck OverturnCheck;

        [Header("Looping")]
        public bool EnableLooping;
        public string LoopTag = "Loop";
        public bool IsLooping;

        [Header("Settings")]
        public bool UseDefaultInputs = true;

        private Transform RotationPivotParent, RotationPivotChild;
        void Start()
        {
            CreateSteeringWheelRotationPivot(SteeringWheel);
            SetVehicleCenterOfMass(VehicleEngine.CenterOfMass);

            if (FrontWheelModel.parent != SteeringWheel)
            {
                FrontWheelModel.parent = SteeringWheel.transform;
            }
            //Create transforms
            RotationPivotParent = new GameObject("Motorcycle Lean Angle Pivot").transform;
            RotationPivotChild = new GameObject("Motorcycle Lean Angle Z").transform;

            RotationPivotChild.SetParent(RotationPivotParent);
            RotationPivotParent.position = transform.position;
            RotationPivotParent.hideFlags = HideFlags.HideInHierarchy;

            //Set Parent
            RotationPivotChild.SetParent(RotationPivotChild);

            //Set Position
            RotationPivotParent.position = transform.position;

            //Hide Rotation Pivot
            RotationPivotParent.hideFlags = HideFlags.HideInHierarchy;

            //DriveVehicles.DriveVehicle(this, FindObjectOfType<JUThirdPersonController>().gameObject);
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
            UpdateWheelModelTransformation(FrontWheel, FrontWheelModel, true);
            UpdateWheelModelTransformation(BackWheel, BackWheelModel);

            //Steering Wheel Rotation
            SteeringWheel.transform.localEulerAngles = SteeringWheelRotation(SteeringWheel, FrontWheel).eulerAngles;
        }
        protected override void VehiclePhysicsUpdate()
        {
            //Turn Off Vehicle
            if (!IsOn)
            {
                //Set Wheels brake
                WheelBrake(FrontWheel);
                WheelBrake(BackWheel);
                return;
            }

            //Set Wheels torque
            if (GroundCheck.IsGrounded)
            {
                WheelTorque(BackWheel);
                WheelTorque(FrontWheel);
            }
            //Set Wheels brake
            WheelBrake(FrontWheel);
            WheelBrake(BackWheel);

            //Set Front Wheel Steer Angle
            float SteerAngleDirection = Mathf.Lerp(GetSmoothedHorizontalMovement(), GetSmoothedHorizontalMovement() / 2.5f, GetVehicleCurrentSpeed(0.1f));
            WheelSteerAngle(FrontWheel, SteerAngleDirection * MaxSteerAngle, MaxSteerAngle);
            //Debug.Log("Vehicle Speed: " + GetVehicleCurrentSpeed().ToString());


            //Inclination Calculation
            if (GetVehicleCurrentSpeed() > 1)
            {
                InclinationValue = GetHorizontalMovement() * GetVehicleCurrentSpeed(2);
            }
            else
            {
                InclinationValue = Mathf.Lerp(InclinationValue, 25, Time.deltaTime);
            }
            InclinationValue = Mathf.Clamp(InclinationValue, -MaxLeanAngle, MaxLeanAngle);

            //Inclination
            if (IsLooping == false)
            {
                MotorcycleLeanSystem();
            }

            //On Air Rotation
            if (GroundCheck.IsGrounded == false) Align(Vector3.up, 0.5f);

            //Limit Speed
            LimitVehicleSpeed(GroundCheck.IsGrounded, false);

            //Loop System
            LoopSystem();
        }
        public Vector3 GetMotorcycleGroundAngle(Vector3 FrontWheelHitNormal, Vector3 BackWheelHitNormal)
        {
            Vector3 Angle = new Vector3((FrontWheelHitNormal.x + BackWheelHitNormal.x) / 2, (FrontWheelHitNormal.y + BackWheelHitNormal.y) / 2, (FrontWheelHitNormal.z + BackWheelHitNormal.z) / 2);
            return Angle;
        }

        protected virtual void MotorcycleLeanSystem()
        {
            //Get wheel ground hits
            RaycastHit FrontWheelHit;
            RaycastHit BackWheelHit;

            //Do raycasts on wheels
            Physics.Raycast(FrontWheelModel.position, -transform.up, out FrontWheelHit, FrontWheel.radius + 0.05f, GroundCheck.RaycastLayerMask);
            Physics.Raycast(BackWheelModel.position, -transform.up, out BackWheelHit, BackWheel.radius + 0.05f, GroundCheck.RaycastLayerMask);

            //Ground Angle
            Vector3 GroundAngle = Vector3.zero;

            //If the two wheel are grouded, create a new ground angle
            if (FrontWheelHit.normal != Vector3.zero && BackWheelHit.normal != Vector3.zero)
            {
                //Create a Ground Angle between wheel ground hits
                GroundAngle = GetMotorcycleGroundAngle(FrontWheelHit.normal, BackWheelHit.normal);
            }
            else
            {
                //Reset Ground Angle, the aligment will be according the GroundCheck Hit
                GroundAngle = Vector3.zero;
            }

            //Simulate Motorcycle Lean System
            SimulateVehicleInclination(InclinationValue, MaxLeanAngle, RotationPivotParent, RotationPivotChild, true, 3, GroundAligment: GroundAngle);
        }
        protected virtual void LoopSystem()
        {
            if (!EnableLooping || GroundCheck.GroundHit.point == Vector3.zero) return;

            //Loop System
            IsLooping = (GroundCheck.GroundHit.collider.tag == LoopTag);

            if (IsLooping)
            {
                Debug.Log("IS LOOPING");
                SimulateGroundAlignment();
            }
        }

        private void OnDrawGizmos()
        {
            VehicleGizmo.DrawVector3Position(CharacterExitingPosition, transform, "Exit Position", Color.green);
            VehicleGizmo.DrawVehicleInclination(RotationPivotParent, RotationPivotChild);
            VehicleGizmo.DrawOverturnCheck(OverturnCheck, transform);
        }
    }

}