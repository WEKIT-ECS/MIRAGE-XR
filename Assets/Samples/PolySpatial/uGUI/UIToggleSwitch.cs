using UnityEngine;

namespace PolySpatial.Samples
{
    public class UIToggleSwitch : MonoBehaviour
    {
        [SerializeField]
        Transform m_KnobTransform;

        const float k_KnobOnPosition = 17f;

        public void MoveKnob(bool isOn)
        {
            if (isOn)
            {
                m_KnobTransform.localPosition = new Vector3(k_KnobOnPosition, 0, 0);
            }
            else
            {
                m_KnobTransform.localPosition = new Vector3(-k_KnobOnPosition, 0, 0);
            }
        }
    }
}
