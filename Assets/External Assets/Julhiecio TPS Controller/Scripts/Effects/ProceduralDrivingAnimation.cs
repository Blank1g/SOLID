using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.ExtendedInverseKinematics;
using JUTPS.VehicleSystem;
using JUTPS.ActionScripts;

namespace JUTPS.FX
{
    [AddComponentMenu("JU TPS/FX/Driver Procedural Animation")]
    [RequireComponent(typeof(DriveVehicles))]
    public class ProceduralDrivingAnimation : JUTPSActions.JUTPSAction
    {
        private DriveVehicles DriveAbility;

        [Header("Settings")]
        public bool Enabled = true;
        public bool FootPlacer;
        private Transform LeftFootTargetPosition, RightFootTargetPosition;

        public LayerMask GroundLayer;
        [Header("Spine Lean")]
        [SerializeField] private bool SpineLean = true;
        [Range(0, 1)]
        [SerializeField] private float LeanDirection = 0.2f;
        [SerializeField] private BodyLeanInert.Axis ForwardLeanAxis = BodyLeanInert.Axis.X;
        [SerializeField] private BodyLeanInert.Axis SidesLeanAxis = BodyLeanInert.Axis.Z;
        public bool InvertForwardLean;
        public bool InvertSideLean;

        private void Start()
        {
            DriveAbility = GetComponent<DriveVehicles>();

            LeftFootTargetPosition = new GameObject("LeftFootTargetPosition").transform;
            RightFootTargetPosition = new GameObject("RightFootTargetPosition").transform;

            LeftFootTargetPosition.hideFlags = HideFlags.HideInHierarchy;
            RightFootTargetPosition.hideFlags = HideFlags.HideInHierarchy;

            LeftFootTargetPosition.position = transform.position;
            RightFootTargetPosition.position = transform.position;

            LeftFootTargetPosition.parent = transform;
            RightFootTargetPosition.parent = transform;

        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!Enabled || DriveAbility == null) return;

            DoProceduralDrivingAnimation(DriveAbility.VehicleToDrive);
        }

        protected virtual void DoProceduralDrivingAnimation(Vehicle Vehicle)
        {
            if (TPSCharacter.IsDriving == false || Vehicle == null || TPSCharacter.IsRagdolled == true) return;


            if (Vehicle.InverseKinematicTargetPositions.LeftFootPositionIK == null ||
               Vehicle.InverseKinematicTargetPositions.RightFootPositionIK == null ||
               Vehicle.InverseKinematicTargetPositions.LeftHandPositionIK == null ||
               Vehicle.InverseKinematicTargetPositions.RightHandPositionIK == null ||
               Vehicle.InverseKinematicTargetPositions.PlayerLocation == null)
            {
                return;
            }

            //Get vehicle magnitude
            //float VehicleMagnitude = Vehicle.rb.velocity.magnitude;
            float VehicleMagnitude = Vehicle.GetVehicleCurrentSpeed();
            VehicleMagnitude = Mathf.Clamp(VehicleMagnitude, 0, 15);

            //Set hands on tags
            anim.SetLeftHandOn(Vehicle.InverseKinematicTargetPositions.LeftHandPositionIK, 1);
            anim.SetRightHandOn(Vehicle.InverseKinematicTargetPositions.RightHandPositionIK, 1);

            //Set procedural hint movements values
            float leftHint = 6 * Mathf.Clamp(Vehicle.GetSmoothedHorizontalMovement(), -1, 0) * VehicleMagnitude / 20;
            float rightHint = 6 * Mathf.Clamp(Vehicle.GetSmoothedHorizontalMovement(), 0, 1) * VehicleMagnitude / 20;

            //Create procedural hint movement
            float HintSpace = 3 * Vehicle.AnimationWeights.HintMovementWeight;
            Vector3 LeftHintLocalPosition = Vector3.zero - Vector3.right * (HintSpace - leftHint) + Vector3.forward * 10;
            Vector3 RightHintLocalPosition = Vector3.zero + Vector3.right * (HintSpace + rightHint) + Vector3.forward * 10;


            //Foot Placer
            if (FootPlacer && Vehicle.AnimationWeights.FootPlacement)
            {
                Vector3 RightFootOriginalPosition = Vehicle.InverseKinematicTargetPositions.RightFootPositionIK.position;
                Vector3 LeftFootOriginalPosition = Vehicle.InverseKinematicTargetPositions.LeftFootPositionIK.position;

                //Set Raycasts
                RaycastHit LeftGroundHit;
                Physics.Raycast(LeftFootOriginalPosition + Vehicle.transform.forward * VehicleMagnitude / 5 - Vehicle.transform.right * 0.2f, -Vehicle.transform.up, out LeftGroundHit, 0.8f, GroundLayer);

                RaycastHit RightGroundHit;
                Physics.Raycast(RightFootOriginalPosition + Vehicle.transform.forward * VehicleMagnitude / 5 + Vehicle.transform.right * 0.2f, -Vehicle.transform.up, out RightGroundHit, 0.8f, GroundLayer);

                //Set ground position
                Vector3 LeftFootOnGroundPosition = LeftGroundHit.collider ? LeftGroundHit.point + LeftGroundHit.normal * 0.15f : LeftFootOriginalPosition;
                Vector3 RightFootOnGroundPosition = RightGroundHit.collider ? RightGroundHit.point + RightGroundHit.normal * 0.15f : RightFootOriginalPosition;

                Vector3 NewLeftFootPosition = Vector3.Lerp(LeftFootOnGroundPosition, LeftFootOriginalPosition, VehicleMagnitude / 5);
                Vector3 NewRightFootPosition = Vector3.Lerp(RightFootOnGroundPosition, RightFootOriginalPosition, VehicleMagnitude / 5);

                Quaternion NewLeftFootRotation = Quaternion.Lerp(Quaternion.FromToRotation(LeftFootTargetPosition.up, LeftGroundHit.normal) * LeftFootTargetPosition.rotation, Vehicle.InverseKinematicTargetPositions.LeftFootPositionIK.rotation, VehicleMagnitude / 5);
                Quaternion NewRightFootRotation = Quaternion.Lerp(Quaternion.FromToRotation(RightFootTargetPosition.up, RightGroundHit.normal) * RightFootTargetPosition.rotation, Vehicle.InverseKinematicTargetPositions.RightFootPositionIK.rotation, VehicleMagnitude / 5);

                LeftFootTargetPosition.position = NewLeftFootPosition; LeftFootTargetPosition.rotation = NewLeftFootRotation;
                RightFootTargetPosition.position = NewRightFootPosition; RightFootTargetPosition.rotation = NewRightFootRotation;

                //Set foot on targets and apply hint movement
                anim.SetLeftFootOn(LeftFootTargetPosition.position, LeftFootTargetPosition.rotation, 1, LeftHintLocalPosition, Vehicle.AnimationWeights.HintMovementWeight);
                anim.SetRightFootOn(Vehicle.InverseKinematicTargetPositions.RightFootPositionIK.position, RightFootTargetPosition.rotation, 1, RightHintLocalPosition, Vehicle.AnimationWeights.HintMovementWeight);
            }
            else
            {
                //Set foot on targets and apply hint movement
                anim.SetLeftFootOn(Vehicle.InverseKinematicTargetPositions.LeftFootPositionIK, 1, LeftHintLocalPosition, Vehicle.AnimationWeights.HintMovementWeight);
                anim.SetRightFootOn(Vehicle.InverseKinematicTargetPositions.RightFootPositionIK, 1, RightHintLocalPosition, Vehicle.AnimationWeights.HintMovementWeight);
            }

            //Spine Lean
            if (!SpineLean) return;

            //Create LookAt Position
            Vector3 LookVehicleDirection = transform.position + Vehicle.transform.forward * 10 + Vehicle.transform.up * 0.6f + Vehicle.transform.right * Vehicle.GetSmoothedHorizontalMovement() * 8;
            //Apply LookAt
            anim.NormalLookAt(LookVehicleDirection, Vehicle.AnimationWeights.LookAtDirectionWeight, 0, 1);


            //Get Lean Values
            float SidewayLeanWeight = -Vehicle.GetSmoothedHorizontalMovement() * (VehicleMagnitude / 5);
            float ForwardLeanWeight = Vehicle.GetSmoothedForwardMovement() * (VehicleMagnitude / 4f);

            Vector3 ForwardAxis = new Vector3(0,0,0);
            switch (ForwardLeanAxis)
            {
                case BodyLeanInert.Axis.X: ForwardAxis = InvertForwardLean ? Vector3.left : Vector3.right;
                    break;
                case BodyLeanInert.Axis.Y: ForwardAxis = InvertForwardLean ? Vector3.down : Vector3.up;
                    break;
                case BodyLeanInert.Axis.Z: ForwardAxis = InvertForwardLean ? Vector3.back : Vector3.forward;
                    break;
            }

            Vector3 SideAxis = new Vector3(0, 0, 0);
            switch (SidesLeanAxis)
            {
                case BodyLeanInert.Axis.X:
                    SideAxis = InvertSideLean ? Vector3.left : Vector3.right;
                    break;
                case BodyLeanInert.Axis.Y:
                    SideAxis = InvertSideLean ? Vector3.down : Vector3.up;
                    break;
                case BodyLeanInert.Axis.Z:
                    SideAxis = InvertSideLean ? Vector3.back : Vector3.forward;
                    break;
            }

            //Apply Forward Lean
            anim.SpineInclination(ForwardAxis, ForwardLeanWeight, Vehicle.AnimationWeights.FrontalLeanWeight);
            //Apply Sideways Lean
            anim.SpineInclination(Vector3.Lerp(Vector3.up, SideAxis, LeanDirection), SidewayLeanWeight, Vehicle.AnimationWeights.SideLeanWeight);
        }
    }
}
