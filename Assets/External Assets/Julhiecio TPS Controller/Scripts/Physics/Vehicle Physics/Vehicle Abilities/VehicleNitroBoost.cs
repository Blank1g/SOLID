using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.VehicleSystem;
using JUTPS.JUInputSystem;

namespace JUTPS.VehicleSystem
{
    public class VehicleNitroBoost : MonoBehaviour
    {
        public bool UseDefaultInput = true;
        public Vehicle.VehicleNitroBoost Nitro;
        [HideInInspector] public bool UseNitro;


        void Update()
        {
            Nitro.SimulateNitro(UseNitro);

            if (!UseDefaultInput) return;

            if (JUInput.GetButtonDown(JUInput.Buttons.RunButton))
            {
                UseNitro = true;
            }
            else
            {
                UseNitro = false;
            }

        }
        public void DoNitro()
        {
            UseNitro = true;
        }
    }

}