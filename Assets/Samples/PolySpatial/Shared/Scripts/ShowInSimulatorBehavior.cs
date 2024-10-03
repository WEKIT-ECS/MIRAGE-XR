using System;
using UnityEngine;

namespace PolySpatial.Samples
{
    class ShowInSimulatorBehavior : MonoBehaviour
    {
        [SerializeField]
        GameObject m_ObjectToShow;

        [SerializeField]
        GameObject m_ObjectToHide;

        void Start()
        {
            var simRoot = Environment.GetEnvironmentVariable("SIMULATOR_ROOT") != null;
            if (m_ObjectToShow != null)
                m_ObjectToShow.SetActive(simRoot);

            if (m_ObjectToHide != null)
                m_ObjectToHide.SetActive(!simRoot);
        }
    }
}
