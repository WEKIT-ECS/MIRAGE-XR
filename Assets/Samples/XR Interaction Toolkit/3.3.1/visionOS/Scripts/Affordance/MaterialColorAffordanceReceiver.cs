using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Affordance receiver that animates a color value on a material instance.
    /// This exists because PolySpatial does not support material property blocks, which is the preferred way of animating material properties.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class MaterialColorAffordanceReceiver : ColorAffordanceReceiver
#pragma warning restore CS0618 // Type or member is obsolete
    {
        [SerializeField]
        Renderer m_Renderer;

        [SerializeField]
        string m_MaterialPropertyName;

        bool m_UseColorDirectly;
        int m_MaterialPropertyID;
        Material m_MaterialInstance;

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            if (m_Renderer == null)
            {
                enabled = false;
                Debug.LogWarning($"No Renderer assigned to {name}. Disabling MaterialColorAffordanceReceiver", this);
                return;
            }

            m_MaterialInstance = m_Renderer.material;
            m_UseColorDirectly = string.IsNullOrEmpty(m_MaterialPropertyName);
            if (!m_UseColorDirectly)
            {
                m_MaterialPropertyID = Shader.PropertyToID(m_MaterialPropertyName);
            }
        }

        /// <inheritdoc />
        protected override void ConsumeAffordance(Color newValue)
        {
            base.ConsumeAffordance(newValue);
            if (m_UseColorDirectly)
                m_MaterialInstance.color = newValue;
            else
                m_MaterialInstance.SetColor(m_MaterialPropertyID, newValue);
        }
    }
}
