using UnityEngine;

namespace PolySpatial.Samples
{
    public class CameraMovementBehavior : MonoBehaviour
    {
        [SerializeField]
        Transform m_VolumeCameraTransfrom;

        [SerializeField]
        Transform m_CharacterTransform;

        [SerializeField]
        float m_CameraDistanceThreshold = 0.3f;

        [SerializeField]
        float m_CameraMovementSpeed = 0.175f;

        float m_CameraHeight;

        void Start()
        {
            m_CameraHeight = m_VolumeCameraTransfrom.position.y;
        }

        void Update()
        {
            if (m_VolumeCameraTransfrom != null && m_CharacterTransform != null)
            {
                var distance = Vector3.Distance(m_VolumeCameraTransfrom.position, m_CharacterTransform.position);

                if (distance >= m_CameraDistanceThreshold)
                {
                    var direction = m_CharacterTransform.position - m_VolumeCameraTransfrom.position;
                    var clampedSpeed = Mathf.Min(distance, m_CameraMovementSpeed * Time.deltaTime);
                    var impulse = direction.normalized * clampedSpeed;
                    m_VolumeCameraTransfrom.position += impulse;
                    m_VolumeCameraTransfrom.position = new Vector3(m_VolumeCameraTransfrom.position.x, m_CameraHeight, m_VolumeCameraTransfrom.position.z);
                }
            }
        }
    }
}
