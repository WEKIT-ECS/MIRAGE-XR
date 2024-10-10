using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Helper class that serves as a way to propagate a Unity event with a string value from a float value.
    /// </summary>
    public class FloatToString : MonoBehaviour
    {
        [SerializeField]
        float m_Value = 0.5f; 
        
        [SerializeField]
        StringUnityEvent m_OnStringValueChanged;
        
        public float value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                var convertedNumber = Mathf.FloorToInt(m_Value * 100f);
                m_OnStringValueChanged?.Invoke(convertedNumber.ToString());
            }
        }
    }
}