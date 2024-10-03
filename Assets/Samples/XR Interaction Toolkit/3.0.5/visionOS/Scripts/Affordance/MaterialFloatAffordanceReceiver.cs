using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Affordance receiver that animates a float value on a material instance.
    /// This exists because PolySpatial does not support material property blocks, which is the preferred way of animating material properties.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class MaterialFloatAffordanceReceiver : FloatAffordanceReceiver
#pragma warning restore CS0618 // Type or member is obsolete
    {
        [SerializeField]
        Renderer m_Renderer;
        
        [SerializeField]
        string m_MaterialPropertyName;
        
        Material m_MaterialInstance;
        int m_MaterialPropertyID;

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            if (m_Renderer == null)
            {
                enabled = false;
                Debug.LogWarning($"No Renderer assigned to {name}. Disabling MaterialFloatAffordanceReceiver", this);
                return;
            }
            m_MaterialInstance = m_Renderer.material;
            m_MaterialPropertyID = Shader.PropertyToID(m_MaterialPropertyName);
        }

        /// <inheritdoc />
        protected override void ConsumeAffordance(float newValue)
        {
            base.ConsumeAffordance(newValue);
            m_MaterialInstance.SetFloat(m_MaterialPropertyID, newValue);
        }
    }
}