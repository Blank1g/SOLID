using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUTPS.VehicleSystem
{

    public class VehicleJump : MonoBehaviour
    {
        private Vehicle vehicle;
        public float JumpForce = 100;
        public bool UseDefaultInput = true;
        void Start()
        {
            vehicle = GetComponent<Vehicle>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!UseDefaultInput) return;

            if (JUInputSystem.JUInput.GetButtonDown(JUInputSystem.JUInput.Buttons.JumpButton))
            {
                Jump(JumpForce);
            }

        }
        public void Jump(float jumpForce)
        {
            if (vehicle == null) return;

            if (vehicle.IsOn == false) return;


            vehicle.Jump(jumpForce, vehicle.GroundCheck.IsGrounded);
        }
    }

}