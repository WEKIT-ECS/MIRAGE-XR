using UnityEngine;

namespace PolySpatial.Samples
{
    public class JointVisuals : MonoBehaviour
    {
        [SerializeField]
        GameObject m_TrackedJoint;

        [SerializeField]
        GameObject m_UntrackedJoint;

        [SerializeField]
        LineRenderer m_Line;

        public LineRenderer Line => m_Line;

        public void SetIsTracked(bool isTracked)
        {
            m_TrackedJoint.SetActive(isTracked);
            m_UntrackedJoint.SetActive(!isTracked);
            m_Line.gameObject.SetActive(isTracked);
        }
    }
}
