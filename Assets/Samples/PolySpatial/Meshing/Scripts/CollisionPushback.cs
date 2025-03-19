using System;
using UnityEngine;

namespace PolySpatial.Samples
{
    public class CollisionPushback : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            Vector3 pushback = -collision.GetContact(0).normal * 5;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(pushback, ForceMode.Impulse);
        }
    }
}
