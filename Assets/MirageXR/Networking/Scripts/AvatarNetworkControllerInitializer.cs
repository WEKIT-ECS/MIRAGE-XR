using UnityEngine;

namespace MirageXR
{
	public class AvatarNetworkControllerInitializer : AvatarInitializer
	{
		[Header("Speaker")]
		[SerializeField] private Transform _speakerInstance;
		[SerializeField] private Vector3 _speakerPositionOffset = new Vector3(0, 0.013f, 0.012f);

		[Header("Name Placement")]
		[SerializeField] private Vector3 nameLabelOffset = new Vector3(0, 0.3f, -0.01f);

		public override int Priority => -10; // after the non-networked initializers

		public override void InitializeAvatar(GameObject avatar)
		{
			NetworkedAvatarReferences avatarRefs = GetComponent<NetworkedAvatarReferences>();

			RelativePositionPlacement relativePositioning = avatarRefs.NameLabel.GetComponent<RelativePositionPlacement>();
			relativePositioning.Target = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			relativePositioning.Offset = nameLabelOffset;

			_speakerInstance.transform.parent = avatarRefs.OfflineReferences.Rig.IK.HeadTarget;
			_speakerInstance.localPosition = _speakerPositionOffset;
			_speakerInstance.localRotation = Quaternion.identity;
		}

		public override void CleanupAvatar(GameObject avatar)
		{
			_speakerInstance.transform.parent = transform;
			_speakerInstance.transform.localPosition = Vector3.zero;
			_speakerInstance.transform.localRotation = Quaternion.identity;
		}
	}
}
