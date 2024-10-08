using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// The poke interaction toggler is a simple script that toggles the poke interactor on and off based on the state of the visionOS touch input.
    /// The interactor is generally designed for continuous input and sweeps the area covered between frames.
    /// Given that the poke transform jumps to the touch position, toggling the poke interactor off and on avoids misbehavior resulting from large sudden sweeps.
    /// </summary>
    public class PokeInteractorToggler : MonoBehaviour
    {
        [SerializeField]
        XRPokeInteractor m_PokeInteractor;
        
        [SerializeField]
        SpatialTouchInputReader m_SpatialTouchInputReader;

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        void OnEnable()
        {
            m_BindingsGroup.AddBinding(m_SpatialTouchInputReader.hasActiveTouch.SubscribeAndUpdate(isActive => m_PokeInteractor.enabled = isActive));
        }

        void OnDisable()
        {
            m_BindingsGroup.Clear();
        }
    }
}
