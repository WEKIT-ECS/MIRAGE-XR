using uLipSync;
using UnityEngine;

namespace MirageXR
{
	/// <summary>
	/// Sets up networking logic for the avatar.
	/// Note that the script does not add anything PhotonBehaviour components as these cannot be created at runtime.
	/// Instead, all the PhotonBehaviour components need to be on the avatar container already and are put in place, e.g., by the rig initializer
	/// </summary>
	public class AvatarNetworkControllerInitializer : AvatarInitializer
	{
		[Header("Voice")]
		[Tooltip("Voice Network Object to which a speaker will attach itself")]
		[SerializeField] private Transform _voiceNetworkObject;
		[Tooltip("Offset from the head IK target so that the speaker ends up at the avatar's mouth")]
		[SerializeField] private Vector3 _speakerPositionOffset = new Vector3(0, 0.013f, 0.012f);

		[Header("Name Placement")]
		[Tooltip("Offset from the head IK target so that the name label ends up above the avatar")]
		[SerializeField] private Vector3 nameLabelOffset = new Vector3(0, 0.3f, -0.01f);

		/// <summary>
		/// Runs after the local avatar initializers
		/// </summary>
		public override int Priority => -10;

		/// <summary>
		/// Connects components related to networking functionality.
		/// For instance, it connects the name label, speaker and lip syncing elements.
		/// </summary>
		/// <param name="avatar">The avatar which should be initialized</param>
		public override void InitializeAvatar(GameObject avatar)
		{
			NetworkedAvatarReferences avatarRefs = GetComponent<NetworkedAvatarReferences>();

			// handle the name label positioning by connecting its relative positioning script
			RelativePositionPlacement relativePositioning = avatarRefs.NameLabel.GetComponent<RelativePositionPlacement>();
			relativePositioning.Target = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			relativePositioning.Offset = nameLabelOffset;

			// make the voice object a part of the avatar by parenting it to the head IK target
			_voiceNetworkObject.transform.parent = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			_voiceNetworkObject.localPosition = _speakerPositionOffset;
			_voiceNetworkObject.localRotation = Quaternion.identity;

			// connect the lip syncing solution to the blend shape driver
			//uLipSync.uLipSync lipSync = _voiceNetworkObject.GetComponent<uLipSync.uLipSync>();
			//uLipSyncBlendShape blendShapeController = avatar.GetComponent<uLipSyncBlendShape>();
			//lipSync.onLipSyncUpdate.AddListener(blendShapeController.OnLipSyncUpdate);
			//blendShapeController.maxBlendShapeValue = 1;
		}

		/// <summary>
		/// Cleans up references related to the networking functionality such as components that need to be reused on the next avatar.
		/// Unparents the voice network object from the avatar and moves it back into the avatar container so that it is not lost when the avatar is destroyed.
		/// Cleans up the lip syncing references.
		/// </summary>
		/// <param name="avatar">The avatar to clean up</param>
		public override void CleanupAvatar(GameObject avatar)
		{
			_voiceNetworkObject.transform.parent = transform;
			_voiceNetworkObject.transform.localPosition = Vector3.zero;
			_voiceNetworkObject.transform.localRotation = Quaternion.identity;

			// disconnect the blend shape driver again to not leave any orphan event listeners
			//uLipSyncBlendShape blendShapeController = avatar.GetComponent<uLipSyncBlendShape>();
			//uLipSync.uLipSync lipSync = _voiceNetworkObject.GetComponent<uLipSync.uLipSync>();
			//lipSync.onLipSyncUpdate.RemoveListener(blendShapeController.OnLipSyncUpdate);
		}
	}
}
