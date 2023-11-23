using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JUTPS.AI
{
    public class SetVehicleRandomDestinations : MonoBehaviour
    {
        private VehicleAI vehicleAI;
        [SerializeField] private float RefreshRate = 10;
        [SerializeField] private float Range = 50;
        private void Start()
        {
            vehicleAI = GetComponent<VehicleAI>();
            InvokeRepeating("Refresh", 0, RefreshRate);
        }

        void Refresh()
        {
            vehicleAI.SetVehicleDestination(new Vector3(Random.Range(-Range, Range), 0, Random.Range(-Range, Range)));
            vehicleAI.RecalculatePath();
        }
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Range * 2, 0, Range * 2));
        }
    }
}