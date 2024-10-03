using System;
using System.Collections;
using UnityEngine;

#if UNITY_INCLUDE_XRI
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace PolySpatial.Samples
{
#if UNITY_INCLUDE_XRI
    [RequireComponent(typeof(LazyFollow))]
#endif
    class DisableFollowAfterDelay : MonoBehaviour
    {
#pragma warning disable CS0414
        [SerializeField]
        float m_DelayInSeconds = 3;
#pragma warning restore CS0414

#if UNITY_INCLUDE_XRI
        IEnumerator Start()
        {
            yield return new WaitForSeconds(m_DelayInSeconds);
            GetComponent<LazyFollow>().positionFollowMode = LazyFollow.PositionFollowMode.None;
        }
#endif
    }
}
