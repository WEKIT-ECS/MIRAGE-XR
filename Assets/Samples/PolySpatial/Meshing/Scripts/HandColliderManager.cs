using System;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

namespace PolySpatial.Samples
{
    public class HandColliderManager : MonoBehaviour
    {
        [SerializeField]
        GameObject m_ColliderPrefab;

        GameObject m_RightIndexCollider;
        GameObject m_LeftIndexCollider;
        GameObject m_RightHandCollider;
        GameObject m_LeftHandCollider;

#if UNITY_INCLUDE_XR_HANDS
        XRHandSubsystem m_HandSubsystem;
        XRHandJoint m_RightIndexTipJoint;
        XRHandJoint m_RightMiddleMetaJoint;
        XRHandJoint m_LeftIndexTipJoint;
        XRHandJoint m_LeftMiddleMetaJoint;

        void Start()
        {
            m_RightIndexCollider = Instantiate(m_ColliderPrefab, Vector3.zero, Quaternion.identity);
            m_LeftIndexCollider = Instantiate(m_ColliderPrefab, Vector3.zero, Quaternion.identity);
            m_RightHandCollider = Instantiate(m_ColliderPrefab, Vector3.zero, Quaternion.identity);
            m_LeftHandCollider = Instantiate(m_ColliderPrefab, Vector3.zero, Quaternion.identity);
        }

        void Update()
        {
            if (TryEnsureInitialized())
            {
                var updateSuccessFlags = m_HandSubsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

                if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != 0)
                {
                    // assign joint values
                    m_RightIndexTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
                    m_RightMiddleMetaJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.MiddleMetacarpal);
                }

                if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != 0)
                {
                    // assign joint values
                    m_LeftIndexTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
                    m_LeftMiddleMetaJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.MiddleMetacarpal);
                }

                // Set the position of the colliders to the joint positions
                SetPrefabPosition(m_RightIndexCollider, m_RightIndexTipJoint);
                SetPrefabPosition(m_RightHandCollider, m_RightMiddleMetaJoint);
                SetPrefabPosition(m_LeftIndexCollider, m_LeftIndexTipJoint);
                SetPrefabPosition(m_LeftHandCollider, m_LeftMiddleMetaJoint);
            }
        }

        bool TryEnsureInitialized()
        {
            if (m_HandSubsystem != null)
                return true;

            m_HandSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (m_HandSubsystem == null)
                return false;

            return true;
        }

        void SetPrefabPosition(GameObject prefab, XRHandJoint joint)
        {
            if (joint.trackingState != XRHandJointTrackingState.None)
            {
                if (joint.TryGetPose(out Pose jointPose))
                {
                    prefab.transform.SetPositionAndRotation(jointPose.position, jointPose.rotation);
                }
            }
        }

#endif
    }
}


