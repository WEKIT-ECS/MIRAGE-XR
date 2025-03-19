using UnityEngine;

namespace PolySpatial.Samples
{
    class ResetObjectZone : MonoBehaviour
    {
        [SerializeField]
        Transform m_RespawnPosition;

        void OnTriggerEnter(Collider other)
        {
            var pieceTransform = other.transform;
            var pieceRigidbody = pieceTransform.GetComponent<Rigidbody>();
            pieceRigidbody.isKinematic = true;
            pieceTransform.position = m_RespawnPosition.position;
            pieceRigidbody.isKinematic = false;
        }
    }
}
