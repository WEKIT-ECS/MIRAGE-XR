namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Simple class that serves to reset the transform of an object to its initial state.
    /// </summary>
    public class Resetable : MonoBehaviour
    {
        Vector3 m_InitialPosition;
        Quaternion m_InitialRotation;
        Vector3 m_InitialScale;
        Rigidbody m_Rigidbody;
        Transform m_Transform;

        void Awake()
        {
            m_Transform = transform;

            // Capture initial transform
            m_InitialPosition = m_Transform.position;
            m_InitialRotation = m_Transform.rotation;
            m_InitialScale = m_Transform.localScale;

            // Get Rigidbody if exists
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public void DoReset()
        {
            // Reset transform
            m_Transform.SetPositionAndRotation(m_InitialPosition, m_InitialRotation);
            m_Transform.localScale = m_InitialScale;

            // Reset Rigidbody velocities if exists
            if (m_Rigidbody != null)
            {
                m_Rigidbody.MovePosition(m_InitialPosition);
                m_Rigidbody.MoveRotation(m_InitialRotation);
                if (!m_Rigidbody.isKinematic)
                    return;

#if UNITY_2023_3_OR_NEWER
                m_Rigidbody.linearVelocity = Vector3.zero;
#else
                m_Rigidbody.velocity = Vector3.zero;
#endif
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
