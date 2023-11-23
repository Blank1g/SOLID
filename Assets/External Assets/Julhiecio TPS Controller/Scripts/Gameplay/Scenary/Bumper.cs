using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUTPS.Utilities
{

    public class Bumper : MonoBehaviour
    {
        public float Force;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = transform.up * Force;
            }
        }
    }

}
