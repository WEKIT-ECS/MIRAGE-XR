using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This static class contains useful helper functions used check and access
    /// bones of a loaded avatar.
    /// </summary>
    public static class AvatarBoneHelper
    {
        private const string ARMATURE_HIPS_LEFT_UP_LEG_BONE_NAME = "Armature/Hips/LeftUpLeg";
        private const string HALF_BODY_LEFT_EYE_BONE_NAME = "Armature/Hips/Spine/Neck/Head/LeftEye";
        private const string FULL_BODY_LEFT_EYE_BONE_NAME = "Armature/Hips/Spine/Spine1/Spine2/Neck/Head/LeftEye";
        private const string HALF_BODY_RIGHT_EYE_BONE_NAME = "Armature/Hips/Spine/Neck/Head/RightEye";
        private const string FULL_BODY_RIGHT_EYE_BONE_NAME = "Armature/Hips/Spine/Spine1/Spine2/Neck/Head/RightEye";

        /// <summary>
        /// This is a legacy function that can be used to check if an
        /// avatar is fullbody and the object does not have an <seealso cref="AvatarData"/> component
        /// </summary>
        /// <param name="avatarRoot">The root transform of the avatar GameObject</param>
        /// <returns>True if it finds a bone only present in full body avatars</returns>
        public static bool IsFullBodySkeleton(Transform avatarRoot)
        {
            return avatarRoot.Find(ARMATURE_HIPS_LEFT_UP_LEG_BONE_NAME);
        }

        /// <summary>
        /// Searches the avatar transform hierarchy for the left eye bone
        /// </summary>
        /// <param name="avatarRoot">The root transform of the avatar GameObject</param>
        /// <param name="isFullBody">Set to true for fullbody avatar.</param>
        /// <returns>The transform of the left eye bone or null if not found.</returns>
        public static Transform GetLeftEyeBone(Transform avatarRoot, bool isFullBody)
        {
            return avatarRoot.Find(isFullBody ? FULL_BODY_LEFT_EYE_BONE_NAME : HALF_BODY_LEFT_EYE_BONE_NAME);
        }

        /// <summary>
        /// Searches the avatar transform hierarchy for the right eye bone
        /// </summary>
        /// <param name="avatarRoot">The root transform of the avatar GameObject</param>
        /// <param name="isFullBody">Set to true for fullbody avatar.</param>
        /// <returns>The transform of the right eye bone or null if not found.</returns>
        public static Transform GetRightEyeBone(Transform avatarRoot, bool isFullBody)
        {
            return avatarRoot.Find(isFullBody ? FULL_BODY_RIGHT_EYE_BONE_NAME : HALF_BODY_RIGHT_EYE_BONE_NAME);
        }
    }
}
