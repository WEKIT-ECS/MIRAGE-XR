using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Reset helper script that resets all objects that have a Resetable component attached.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class ResetAll : MonoBehaviour
    {
        List<Resetable> m_Resetables = new List<Resetable>();
        XRBaseInteractable m_Interactable;
        
        void Awake()
        {
            m_Resetables.AddRange(FindObjectsOfType<Resetable>());
            m_Interactable = GetComponent<XRBaseInteractable>();
            m_Interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            // If the interactor is a poke interactor, reset all objects.
            if (args.interactorObject is IPokeStateDataProvider)
                ResetAllObjects();
        }

        public void ResetAllObjects()
        {
            foreach (var resetable in m_Resetables)
            {
                resetable.DoReset();
            }
        }
    }
}