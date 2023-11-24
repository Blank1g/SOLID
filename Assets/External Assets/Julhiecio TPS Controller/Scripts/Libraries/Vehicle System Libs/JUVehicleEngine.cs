using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUTPS.VehicleSystem
{
	public class Vehicle : MonoBehaviour
	{
		[HideInInspector] public Rigidbody rb;
		//protected RaycastHit groundHit;

		protected float _horizontal;
		protected float _vertical;

		protected float _smoothedHorizontal;
		protected float _smoothedForward;

		protected float _inclination;
		protected bool _brake;

		private Vector3 _oldPosition;
		private float _currentMagnitude;

		[Header("Vehicle Locomotion Settings")]
		public Vector3 CharacterExitingPosition = new Vector3(-1, 0, 0);
		public VehicleEngineSettings VehicleEngine;

		[Header("Steering Wheel Settings")]
		public GameObject SteeringWheel;
		public float MaxSteerAngle = 20;

		[Header("Ground Check")]
		public VehicleGroundCheck GroundCheck;

		[Header("Driver Inverse Kinematics")]
		public IKTargetPositions InverseKinematicTargetPositions;

		[Header("Driver Procedural Animation Weights")]
		public DrivingProceduralAnimationWeights AnimationWeights;

		[Header("Vehicle State")]
		public bool IsOn;

		protected virtual void Awake()
		{
			rb = GetComponent<Rigidbody>();
		}
		protected virtual void Update()
		{
			VehicleUpdate();

			_smoothedForward = Mathf.Lerp(_smoothedForward, GetForwardAxisPhysicalMovement(), 5 * Time.deltaTime);
			_smoothedHorizontal = Mathf.Lerp(_smoothedHorizontal, GetHorizontalMovement(), 5 * Time.deltaTime);

			//Update Vehicle Current Speed
			if (rb == null)
			{
				_currentMagnitude = Mathf.Lerp(_currentMagnitude, (transform.position - _oldPosition).magnitude * 100f, 10 * Time.deltaTime);
				_oldPosition = transform.position;
			}
			else
			{
				_currentMagnitude = Mathf.Lerp(_currentMagnitude, rb.velocity.magnitude, 10 * Time.deltaTime);
			}
		}
		protected virtual void FixedUpdate()
		{
			VehiclePhysicsUpdate();
		}
		protected virtual void VehicleUpdate()
		{

		}
		protected virtual void VehiclePhysicsUpdate()
		{

		}

		// >>> Controller Utility Methods

		public virtual void SetEngineInputs(float HorizontalInput, float VerticalInput, bool BrakeInput)
		{
			_horizontal = HorizontalInput;
			_vertical = VerticalInput;
			_brake = BrakeInput;
		}
		public void GetEngineInputs(out float HorizontalInput, out float VerticalInput, out bool BrakeInput) { HorizontalInput = _horizontal; VerticalInput = _vertical; BrakeInput = _brake; }
		public float GetHorizontalInput() { return _horizontal; }
		public float GetVerticalInput() { return _vertical; }
		public bool GetBrakeInput() { return _brake; }

		public virtual void Jump(float JumpForce, bool IsGrounded)
		{
			if (IsGrounded == false) return;

			rb.AddRelativeForce(0, JumpForce, 0, ForceMode.Impulse);
		}


		// >>> Steering Wheel Utility Methods

		protected Quaternion SteeringWheelRotation(GameObject SteeringWheel, WheelCollider WheelToGetSteerAngle, float MultiplySteeringWheelRotation = 1)
		{
			//var SteeringWheelRotation = MaxSteerAngle * (SmoothedHorizontalValue ? _smoothedHorizontal : _horizontal);
			var SteeringWheelRotation = MultiplySteeringWheelRotation * WheelToGetSteerAngle.steerAngle;
			Vector3 RotationEuler = new Vector3(SteeringWheel.transform.localEulerAngles.x, SteeringWheelRotation, SteeringWheel.transform.localEulerAngles.x);
			return Quaternion.Euler(RotationEuler);
		}
		protected Quaternion SteeringWheelRotation(GameObject SteeringWheel, float MultiplySteeringWheelRotation = 1)
		{
			var SteeringWheelRotation = MultiplySteeringWheelRotation * MaxSteerAngle * (_smoothedHorizontal);
			//var SteeringWheelRotation = MultiplySteeringWheelRotation * MaxSteerAngle;
			Vector3 RotationEuler = new Vector3(SteeringWheel.transform.localEulerAngles.x, SteeringWheelRotation, SteeringWheel.transform.localEulerAngles.x);
			return Quaternion.Euler(RotationEuler);
		}

		protected virtual void CreateSteeringWheelRotationPivot(GameObject SteeringWheel)
		{
			//Create a pivot and setup transformations
			GameObject SteeringWheelRotationAxisFix = new GameObject("Steering Wheel");
			SteeringWheelRotationAxisFix.transform.position = SteeringWheel.transform.position;
			SteeringWheelRotationAxisFix.transform.rotation = SteeringWheel.transform.rotation;
			SteeringWheelRotationAxisFix.transform.parent = SteeringWheel.transform.parent;

			//Steering wheel parenting with the pivot
			SteeringWheel.transform.parent = SteeringWheelRotationAxisFix.transform;
		}
		protected virtual void CreateSteeringWheelRotationPivot(GameObject SteeringWheel, out GameObject SteeringWheelRotationPivot)
		{
			//Create a pivot and setup transformations
			GameObject SteeringWheelRotationAxisFix = new GameObject("Steering Wheel");
			SteeringWheelRotationAxisFix.transform.position = SteeringWheel.transform.position;
			SteeringWheelRotationAxisFix.transform.rotation = SteeringWheel.transform.rotation;
			SteeringWheelRotationAxisFix.transform.parent = SteeringWheel.transform.parent;

			//Output pivot
			SteeringWheelRotationPivot = SteeringWheelRotationAxisFix;

			//Steering wheel parenting with the pivot
			SteeringWheel.transform.parent = SteeringWheelRotationAxisFix.transform;
		}


		// >>> Wheel Physics Utility Methods

		protected void WheelSteerAngle(WheelCollider wheel, float SteerAngle, float MaxSteerAngle = 45)
		{
			SteerAngle = Mathf.Clamp(SteerAngle, -MaxSteerAngle, MaxSteerAngle);
			wheel.steerAngle = SteerAngle;
		}
		protected void WheelTorque(WheelCollider wheel)
		{
			wheel.motorTorque = _vertical * VehicleEngine.TorqueForce;
		}
		protected void WheelBrake(WheelCollider wheel)
		{
			float InMovementBrake = (_brake || !IsOn) ? VehicleEngine.BrakeForce : 0;
			float StoppingBrake = (_vertical == 0) ? (VehicleEngine.BrakeForce / 15) : InMovementBrake;
			wheel.brakeTorque = _brake ? InMovementBrake : StoppingBrake;
		}
		protected void UpdateWheelModelTransformation(WheelCollider wheel_collider, Transform wheel_model, bool JustRotateOnXAxist = false)
		{
			Vector3 pos;
			Quaternion rot;
			wheel_collider.GetWorldPose(out pos, out rot);

			//Apply position
			wheel_model.position = pos;

			//Apply Rotation
			if (JustRotateOnXAxist)
			{
				//get euler angles
				Vector3 wheelLocalEulerAngles = rot.eulerAngles;
				//Reset some axis
				wheelLocalEulerAngles.y = 0;
				wheelLocalEulerAngles.z = 0;

				//apply modified local euler angles
				wheel_model.localEulerAngles = wheelLocalEulerAngles;

				//wheel_model.rotation = rot;

				//wheel_model.localEulerAngles = new Vector3(wheel_model.localEulerAngles.x, 0, 0);
			}
			else
			{
				wheel_model.rotation = rot;
			}
		}

		// >>> Vehicle Physics Utility Methods

		protected void AddForwardAcceleration(float AccelerationForce)
		{
			if (!IsOn) return;
			rb.AddRelativeForce(0, 0, AccelerationForce, ForceMode.Acceleration);
		}
		protected void LimitVehicleSpeed(bool IsGrounded = true, bool LimitGravity = false)
		{
			if (IsGrounded == false) return;
			if (rb.velocity.magnitude > VehicleEngine.MaxVelocity)
			{
				//Get rb velocity
				Vector3 RigidBodyVelocity = rb.velocity;
				//Clamp rb velocity magnitude
				RigidBodyVelocity = Vector3.ClampMagnitude(RigidBodyVelocity, VehicleEngine.MaxVelocity);

				if (LimitGravity == false)
				{
					//Restore fall speed
					RigidBodyVelocity.y = rb.velocity.y;
				}

				//Apply Limitation
				rb.velocity = RigidBodyVelocity;
			}
		}
		protected void SimulateAntiRollBar(float AntiRollForce, WheelCollider LeftWheel, WheelCollider RightWheel)
		{
			//Wheel hits
			WheelHit hit;

			//Right Wheel AntiRoll Weight 
			float LeftWheelAntiRollBarWeight = 1.0f;
			//Left Wheel AntiRoll Weight 
			float RightWheelAntiRollBarWeight = 1.0f;

			//Get Left Wheel Ground Hit
			bool LeftWheelIsGrounded = LeftWheel.GetGroundHit(out hit);
			if (LeftWheelIsGrounded)
			{
				//Set Left Side Anti-Roll Bar Force
				LeftWheelAntiRollBarWeight = (-LeftWheel.transform.InverseTransformPoint(hit.point).y - LeftWheel.radius) / LeftWheel.suspensionDistance;
			}

			//Get Right Wheel Ground Hit
			bool RightWheelIsGrounded = RightWheel.GetGroundHit(out hit);
			if (RightWheelIsGrounded)
			{
				//Set Right Side Anti-Roll Bar Force
				RightWheelAntiRollBarWeight = (-RightWheel.transform.InverseTransformPoint(hit.point).y - RightWheel.radius) / RightWheel.suspensionDistance;
			}

			//Get Final Anti-Roll Force 
			float AntiRollForceToApply = (LeftWheelAntiRollBarWeight - RightWheelAntiRollBarWeight) * AntiRollForce;

			// >>> Apply Anti-Roll bar Simulation
			if (LeftWheelIsGrounded) rb.AddForceAtPosition(LeftWheel.transform.up * -AntiRollForceToApply, LeftWheel.transform.position);
			if (RightWheelIsGrounded) rb.AddForceAtPosition(RightWheel.transform.up * AntiRollForceToApply, RightWheel.transform.position);
		}
		protected void SetVehicleCenterOfMass(Transform position)
		{
			if (position == null) return;
			if (position.parent != this.transform) { Debug.LogWarning("Failed to set Vehicle Center of Mass because the selected Transform is not a child of the Vehicle"); return; }

			rb.centerOfMass = position.localPosition;
		}
		public float GetVehicleCurrentSpeed(float Multiplier = 1)
		{
			return (_currentMagnitude * Multiplier > 0.0001f) ? _currentMagnitude * Multiplier : 0;
		}


		// >>> Vehicle Alignment Utility Methods

		protected void SimulateVehicleInclination(float InclinationValue, float MaxInclinationAngle, Transform RotationPivotParent, Transform RotationPivotChild, bool FreezeRotationToBetterSimulation = true, float SimulationForce = 8f, float OnGroundRigidbodyDrag = 5f, float OffGroundRigidbodyDrag = 1f, Vector3 GroundAligment = default(Vector3))
		{
			//Do not execute the inclination if the parent is not correct
			if (RotationPivotChild.parent != RotationPivotParent)
			{
				Debug.LogError("The parent of the RotationPivotChild variable is not the same GameObject as the RotationPivotParent");
				return;
			}

			//Get Inclination Values
			float ForwardInclination = GetForwardAxisPhysicalMovement() + (rb.velocity.magnitude / 10) * -InclinationValue;
			float BackwardInclination = (-GetForwardAxisPhysicalMovement() / 2) + (rb.velocity.magnitude / 10) * -InclinationValue;

			//Apply forward inclination
			if (GetForwardAxisPhysicalMovement() > 0) _inclination = Mathf.Lerp(_inclination, ForwardInclination, 8f * Time.deltaTime);
			//Apply backward inclination
			if (GetForwardAxisPhysicalMovement() < -0) _inclination = Mathf.Lerp(_inclination, BackwardInclination, 8f * Time.deltaTime);
			//Apply idle inclination
			if (GetVehicleCurrentSpeed() == 0) _inclination = Mathf.Lerp(_inclination, InclinationValue, 8f * Time.deltaTime);

			//Limit inclination
			_inclination = Mathf.Clamp(_inclination, -MaxInclinationAngle, MaxInclinationAngle);
			//Disable inclination on vehicle brake
			_inclination = _brake ? 0 : _inclination;

			//Create a euler angles value with inclination on z axis
			var childEulerRotation = Vector3.zero;
			childEulerRotation.z = _inclination;
			//Apply local euler angles
			RotationPivotChild.localEulerAngles = new Vector3(0, 0, _inclination);

			//Set RotationPivot to vehicle transform
			RotationPivotParent.position = transform.position;
			//Set RotationPivot to align with the ground
			RotationPivotParent.rotation = Quaternion.Slerp(RotationPivotParent.rotation, Quaternion.FromToRotation(RotationPivotParent.up, (GroundAligment != Vector3.zero) ? GroundAligment : GroundCheck.GroundHit.normal) * RotationPivotParent.rotation, 5 * Time.deltaTime);
			//Set RotationPivot forward tha same vehicle forward;
			RotationPivotParent.localEulerAngles = new Vector3(RotationPivotParent.localEulerAngles.x, transform.localEulerAngles.y, RotationPivotParent.localEulerAngles.z);

			//Apply Final Rotation
			transform.rotation = Quaternion.Lerp(transform.rotation, RotationPivotChild.rotation, SimulationForce * Time.deltaTime);

			//Freeze rigidbody rotation
			if (FreezeRotationToBetterSimulation == false) return;

			if (GroundCheck.IsGrounded)
			{
				rb.angularDrag = OnGroundRigidbodyDrag;
				rb.constraints = RigidbodyConstraints.FreezeRotationZ;
			}
			else
			{
				rb.angularDrag = OffGroundRigidbodyDrag;
				rb.constraints = RigidbodyConstraints.None;
			}
		}
		protected void SimulateGroundAlignment(float AlignmentSpeed = 8)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, GroundCheck.GroundHit.normal) * transform.rotation, AlignmentSpeed * Time.deltaTime);
		}
		protected void Align(Vector3 Normal, float AlignmentSpeed = 1)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, Normal) * transform.rotation, AlignmentSpeed * Time.deltaTime);
		}


		// >>> Vehicle Direction Utility Methods

		/// <summary>
		/// Will return a value between 1 and -1, where 1 is the same direction and -1 is the opposite direction.
		/// So: 1 = Forward; -1 = Backward;
		/// </summary>
		public float GetForwardAxisPhysicalMovement()
		{
			float dir = 0;
			if (rb != null)
			{
				dir = Vector3.Dot(transform.forward, rb.velocity.normalized);
				return dir;
			}
			return dir;
		}
		/// <summary>
		/// Will return a value between 1 and -1, where 1 is the right direction and -1 is the left direction.
		/// </summary>
		public float GetHorizontalMovement()
		{
			return _horizontal;
		}
		/// <summary>
		/// Will return a value between 1 and -1, where 1 is the same direction and -1 is the opposite direction.
		/// So: 1 = Forward; -1 = Backward;
		/// </summary>
		public float GetSmoothedForwardMovement()
		{
			return _smoothedForward;
		}
		/// <summary>
		/// Will return a value between 1 and -1, where 1 is the same direction and -1 is the opposite direction.
		/// So: 1 = Forward; -1 = Backward;
		/// </summary>
		public float GetSmoothedHorizontalMovement()
		{
			return _smoothedHorizontal;
		}


		public Vector3 GetExitPosition()
		{
			Vector3 exitPosition = CharacterExitingPosition;

			Vector3 oppositeExitPosition = CharacterExitingPosition;
			oppositeExitPosition.x = -CharacterExitingPosition.x;

			Vector3 RaycastOriginPosition = transform.position + transform.forward * CharacterExitingPosition.z + transform.up * CharacterExitingPosition.y;

			bool ExitPositionAvaliable = !Physics.Raycast(RaycastOriginPosition, -transform.right, Mathf.Abs(CharacterExitingPosition.x), GroundCheck.RaycastLayerMask);
			bool OppositeExitPositionAvaliable = !Physics.Raycast(RaycastOriginPosition, transform.right, Mathf.Abs(CharacterExitingPosition.x), GroundCheck.RaycastLayerMask);

			Vector3 finalExitPosition = Vector3.zero;

			if (ExitPositionAvaliable)
			{
				finalExitPosition = exitPosition;
			}
			if (OppositeExitPositionAvaliable && ExitPositionAvaliable == false)
			{
				finalExitPosition = oppositeExitPosition;
			}
			if (OppositeExitPositionAvaliable == false && ExitPositionAvaliable == false)
			{
				finalExitPosition = Vector3.zero;
			}



			if (finalExitPosition != Vector3.zero)
			{
				return transform.TransformPoint(finalExitPosition);
			}
			else
			{
				return Vector3.zero;
			}
		}



		// >>> Vehicle Utility Classes

		[System.Serializable]
		public class VehicleNitroBoost
		{
			public Rigidbody RigidbodyToBoost;
			public float NitroForce = 200;

			//public float NitroDuration;
			//public float NitroRechargeDuration;


			//private float CurrentNitroDuration;
			//private float CurrentTimeToRechargeNitro;
			public float NitroBar = 1;

			public float SpendNitroSpeed = 2;
			public float RechargeNitroSpeed = 0.5f;

			public GameObject NitroParticle;

			public bool CanUseNitroState;
			public bool UsingNitroState;

			public void SimulateNitro(bool NitroInputValue)
			{
				if (RigidbodyToBoost == null) return;

				if (UsingNitroState == true)
				{
					//Boost
					RigidbodyToBoost.AddRelativeForce(0, 0, NitroForce, ForceMode.Acceleration);
					//Timer
					NitroBar -= Time.deltaTime;
					//On Nitro ends
					if (NitroBar <= 0)
					{
						Debug.Log("Nitro ended");
						CanUseNitroState = false;
						UsingNitroState = false;
					}
				}

				if (CanUseNitroState == true && UsingNitroState == false)
				{
					//if press nitro input key
					if (NitroInputValue)
					{
						//Impulse
						RigidbodyToBoost.AddRelativeForce(0, 0, NitroForce, ForceMode.Impulse);
						//Block using nitro agaim
						CanUseNitroState = false;
						//Enable Nitro
						UsingNitroState = true;
					}
				}
				else if (UsingNitroState == false)
				{
					//Recharge Nitro
					NitroBar += RechargeNitroSpeed * Time.deltaTime;

					if (NitroBar > 1)
					{
						NitroBar = 1;
						CanUseNitroState = true;
						Debug.Log("Nitro Fully");
					}
					//On Recharge Nitro
				}

				//Enable/Disable Nitro Particle
				if (NitroParticle != null) NitroParticle.SetActive(UsingNitroState);
			}
			/*
			public void SimulateNitro(bool NitroInputValue)
			{
				if (RigidbodyToBoost == null) return;

				if (UsingNitroState == true)
				{
					//Boost
					RigidbodyToBoost.AddRelativeForce(0, 0, NitroForce, ForceMode.Acceleration);
					//Timer
					CurrentNitroDuration += Time.deltaTime;
					//On timer end counting
					if (CurrentNitroDuration > NitroDuration)
					{
						CurrentNitroDuration = 0; 
						UsingNitroState = false;
					}
				}

				if (CanUseNitroState == true && UsingNitroState == false)
				{
					//if press nitro input key
					if (NitroInputValue)
					{
						//Impulse
						RigidbodyToBoost.AddRelativeForce(0, 0, NitroForce, ForceMode.Impulse);
						//Block using nitro agaim
						CanUseNitroState = false;
						//Enable Nitro
						UsingNitroState = true;
					}
				}
				else if(UsingNitroState == false)
				{
					//Recharge Nitro Timer
					CurrentNitroDuration += Time.deltaTime;
					//On Recharge Nitro Timer end couting
					if (CurrentNitroDuration >= CurrentTimeToRechargeNitro)
					{
						CurrentNitroDuration = 0; CanUseNitroState = true;
					}
				}
				//Nitro Bar
				CurrentNitroBar = Mathf.Lerp(1,0, CurrentNitroDuration / NitroDuration);
				//Enable/Disable Nitro Particle
				if (NitroParticle != null) NitroParticle.SetActive(UsingNitroState);
			}
			*/
		}

		[System.Serializable]
		public class VehicleGroundCheck
		{
			[Header("Ground Check")]
			public Vector3 RaycastOrigin;
			public float RaycastDistance;
			public LayerMask RaycastLayerMask;

			[Header("State")]
			public bool IsGrounded;

			public RaycastHit GroundHit;

			public void GroundCheck(Transform vehicle, Vector3 VehicleDownDirection = default(Vector3))
			{
				Vector3 ray_origin = vehicle.position + (vehicle.right * RaycastOrigin.x) + (vehicle.up * RaycastOrigin.y) + (vehicle.forward * RaycastOrigin.z);
				if (Physics.Raycast(ray_origin, (VehicleDownDirection == Vector3.zero) ? -vehicle.up : VehicleDownDirection, out GroundHit, RaycastDistance, RaycastLayerMask))
				{
					IsGrounded = true;
				}
				else
				{
					IsGrounded = false;
				}
			}
		}

		[System.Serializable]
		public class VehicleOverturnCheck
		{
			[Header("Overturn Check")]
			public Vector3 CheckboxPosition = new Vector3(0, 1.5f, 0);
			public Vector3 CheckboxScale = new Vector3(0.4f, 0.3f, 1.5f);
			public LayerMask CheckboxLayerMask;

			[Header("Anti-Overturn")]
			public bool EnableAntiOverturn;
			public float AntiOverturnSpeed = 1f;

			[Header("State")]
			public bool IsOverturned;

			private RaycastHit GroundHit;

			public void OverturnCheck(Transform vehicle)
			{
				Vector3 origin = vehicle.position + vehicle.right * CheckboxPosition.x + vehicle.up * CheckboxPosition.y + vehicle.forward * CheckboxPosition.z;
				var checkbox = Physics.OverlapBox(origin, CheckboxScale, vehicle.rotation, CheckboxLayerMask);
				IsOverturned = (checkbox.Length != 0);

				//if(IsOverturned)Debug.Log("Is overturned!");
			}
			public void AntiOverturn(Transform vehicle)
			{
				if (!IsOverturned) return;
				vehicle.rotation = Quaternion.Lerp(vehicle.rotation, Quaternion.FromToRotation(vehicle.up, GroundHit.normal) * vehicle.rotation, AntiOverturnSpeed * Time.deltaTime);
			}
		}

		[System.Serializable]
		public class VehicleOverlapBoxCheck
		{
			[Header("Overturn Check")]
			public Vector3 CheckboxPosition = new Vector3(0, 0, 0);
			public Vector3 CheckboxScale = new Vector3(0.4f, 0.4f, 0.4f);
			public LayerMask CheckboxLayerMask;

			[Header("State")]
			public bool Collided;

			private RaycastHit GroundHit;

			public void Check(Transform vehicle)
			{
				Vector3 origin = vehicle.position + vehicle.right * CheckboxPosition.x + vehicle.up * CheckboxPosition.y + vehicle.forward * CheckboxPosition.z;
				var checkbox = Physics.OverlapBox(origin, CheckboxScale, vehicle.rotation, CheckboxLayerMask);
				Collided = (checkbox.Length != 0);
				if (Collided) Debug.Log("collided");
			}
		}

		[System.Serializable]
		public class VehicleRaycastCheck
		{
			[Header("Raycast Check")]
			public Vector3 OriginPosition = new Vector3(0, 0, 0);
			public float RayMaxDistance = 1;
			public LayerMask RayLayerMask;

			[Header("State")]
			public bool IsCollided;

			public RaycastHit raycastHit;

			public void Check(Transform vehicle, Vector3 direction)
			{
				Vector3 origin = vehicle.position + vehicle.right * OriginPosition.x + vehicle.up * OriginPosition.y + vehicle.forward * OriginPosition.z;
				IsCollided = Physics.Raycast(origin, direction, out raycastHit, RayMaxDistance, RayLayerMask);
			}
		}

		[System.Serializable]
		public class IKTargetPositions
		{
			public Transform PlayerLocation;
			public Transform LeftHandPositionIK;
			public Transform RightHandPositionIK;
			public Transform LeftFootPositionIK;
			public Transform RightFootPositionIK;
		}

		[System.Serializable]
		public class VehicleEngineSettings
		{
			public float MaxVelocity = 160;
			public float TorqueForce = 2000;
			public float BrakeForce = 8000;
			public Transform CenterOfMass;
		}

		[System.Serializable]
		public class DrivingProceduralAnimationWeights
		{
			public float FrontalLeanWeight;
			public float SideLeanWeight;
			public float LookAtDirectionWeight;
			public float HintMovementWeight;
			public bool FootPlacement = false;
		}

		public static class VehicleGizmo
		{
			public static void DrawVector3Position(Vector3 position, Transform Vehicle, string Label = "", Color color = default(Color))
			{
				if (color != Color.clear)
				{
					Gizmos.color = color;
				}

				Vector3 wordlPosition = Vehicle.position + Vehicle.right * position.x + Vehicle.up * position.y + Vehicle.forward * position.z;
				if (Label != "")
				{
#if UNITY_EDITOR
					UnityEditor.Handles.Label(wordlPosition + Vector3.up * 0.1f, Label);
#endif
				}
				Gizmos.DrawSphere(wordlPosition, 0.03f);
			}
			public static void DrawVehicleInclination(Transform RotationParent, Transform RotationChild)
			{
				if (RotationChild == null || RotationParent == null) return;

				//Points
				Vector3 pos1 = RotationParent.position + RotationParent.up * 1;
				Vector3 pos2 = RotationChild.position + RotationChild.up * 1;

				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(pos1, 0.01f);
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(pos2, 0.01f);

				//Base
				Gizmos.color = Color.grey;
				Gizmos.DrawRay(RotationParent.position, RotationParent.up);

				//Real rotation
				Gizmos.color = Color.green;
				Gizmos.DrawRay(RotationChild.position, RotationChild.up);
#if UNITY_EDITOR

				//Infos
				UnityEditor.Handles.Label(RotationParent.position + RotationChild.up * 1.2f, Vector3.Angle(RotationChild.up, RotationParent.up).ToString("00.0"));
#endif

				//Base Lines
				Gizmos.color = Color.green;
				Gizmos.DrawLine(RotationParent.position - RotationParent.right, RotationParent.position + RotationParent.right);
#if UNITY_EDITOR

				//Disc
				UnityEditor.Handles.color = Color.green;
				UnityEditor.Handles.DrawWireArc(RotationParent.position, RotationParent.forward, RotationParent.right, 180, 1);
#endif
			}
			public static void DrawRaycastHit(Vehicle.VehicleRaycastCheck rayCheck, Transform vehicle, Vector3 direction)
			{
				Gizmos.color = rayCheck.IsCollided ? Color.green : Color.red;

				Vector3 origin = vehicle.position + vehicle.right * rayCheck.OriginPosition.x + vehicle.up * rayCheck.OriginPosition.y + vehicle.forward * rayCheck.OriginPosition.z;

				Gizmos.DrawLine(origin, rayCheck.IsCollided ? rayCheck.raycastHit.point : origin + direction * rayCheck.RayMaxDistance);
			}
			public static void DrawOverturnCheck(Vehicle.VehicleOverturnCheck OverturnCheck, Transform Vehicle)
			{
				if (OverturnCheck.EnableAntiOverturn)
				{
					Gizmos.matrix = Matrix4x4.TRS(Vehicle.position, Vehicle.rotation, Vehicle.localScale);
					Gizmos.color = OverturnCheck.IsOverturned ? Color.green : Color.red;
					Gizmos.DrawWireCube(Vector3.zero + Vector3.up * OverturnCheck.CheckboxPosition.y + Vector3.right * OverturnCheck.CheckboxPosition.x + Vector3.forward * OverturnCheck.CheckboxPosition.z, OverturnCheck.CheckboxScale);
				}
			}
			public static void DrawOverlapBoxCheck(Vehicle.VehicleOverlapBoxCheck BoxCheck, Transform Vehicle)
			{
				Gizmos.matrix = Matrix4x4.TRS(Vehicle.position, Vehicle.rotation, Vehicle.localScale);
				Gizmos.color = BoxCheck.Collided ? Color.green : Color.red;
				Gizmos.DrawWireCube(Vector3.zero + Vector3.up * BoxCheck.CheckboxPosition.y + Vector3.right * BoxCheck.CheckboxPosition.x + Vector3.forward * BoxCheck.CheckboxPosition.z, BoxCheck.CheckboxScale);
			}
			public static void DrawVehicleGroundCheck(Vehicle.VehicleGroundCheck GroundCheck, Transform Vehicle)
			{
				Gizmos.color = GroundCheck.IsGrounded ? Color.green : Color.red;
				Gizmos.DrawLine(Vehicle.position + GroundCheck.RaycastOrigin, Vehicle.position + GroundCheck.RaycastOrigin - Vehicle.up * GroundCheck.RaycastDistance);
			}
		}
	}
}
