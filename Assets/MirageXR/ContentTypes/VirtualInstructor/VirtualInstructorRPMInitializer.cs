using Cysharp.Threading.Tasks;
using uLipSync;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirageXR
{
	public class VirtualInstructorRPMInitializer : AvatarInitializer
	{
		// run this last
		public override int Priority => -100;

		public Profile LipSyncProfile { get; set; }
		public Vector3 SpeakerPositionOffset = new Vector3(0, 0.013f, 0.012f);

		public override void InitializeAvatar(GameObject avatar)
		{
			AvatarVisibilityController visibilityController = gameObject.GetComponent<AvatarVisibilityController>();
			visibilityController.FadeVisibility = false;
			visibilityController.Visible = false;
			visibilityController.FadeVisibility = true;
			visibilityController.Visible = true;

			AvatarReferences avatarRefs = GetComponentInParent<AvatarReferences>();

			GameObject speaker = new GameObject("Speaker");
			speaker.transform.SetParent(avatarRefs.Rig.IK.HeadTarget);
			speaker.transform.position = SpeakerPositionOffset;

			// audio setup
			avatarRefs.Speaker = speaker.AddComponent<AudioSource>();
			avatarRefs.Speaker.spatialBlend = 1f;
			avatarRefs.Speaker.minDistance = 1;
			avatarRefs.Speaker.maxDistance = 20;
			avatarRefs.Speaker.rolloffMode = AudioRolloffMode.Linear;
		}

		public async override UniTask InitializeAvatarAsync(GameObject avatar)
		{
			AvatarReferences avatarReferences = GetComponent<AvatarReferences>();
			// disable IK
			avatarReferences.Rig.IK.RigLayer.weight = 0;
			avatarReferences.Rig.IK.HeadConstraint.weight = 0;
			avatarReferences.Rig.IK.HipsConstraint.weight = 0;
			avatarReferences.Rig.IK.HipsTarget.GetComponent<BodyController>().enabled = false;
			for (int i = 0; i < 2; i++)
			{
				avatarReferences.Rig.IK.GetSide(i).Hand.Constraint.weight = 0;
				avatarReferences.Rig.IK.GetSide(i).Hand.Target.GetComponent<HandController>().enabled = false;
				avatarReferences.Rig.IK.GetSide(i).Hand.Target.GetComponent<HandJointsController>().enabled = false;
				avatarReferences.Rig.IK.GetSide(i).Foot.Constraint.weight = 0;
				avatarReferences.Rig.IK.GetSide(i).Foot.Target.GetComponent<FootController>().enabled = false;
			}
			TurnToUser turnToUser = avatar.AddComponent<TurnToUser>();
			turnToUser.RotationOffset = Quaternion.Euler(0, 180, 0);
			Animator animator = avatar.GetComponent<Animator>();
			string animatorControllerPath = "ReadyPlayerMe/AnimatorController";
			var handle = Addressables.LoadAssetAsync<RuntimeAnimatorController>(animatorControllerPath);
			await handle.Task;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				animator.runtimeAnimatorController = handle.Result;
			}
			else
			{
				Debug.LogError("Something went wrong loading the animator controller for the Ready Player Me character", this);
			}

			// lip sync setup
			uLipSync.uLipSync lipSync = avatarReferences.Speaker.gameObject.AddComponent<uLipSync.uLipSync>();
			string lipSyncProfilePath = "Avatar/LipSyncProfile";
			var profileLoadHandle = Addressables.LoadAssetAsync<Profile>(lipSyncProfilePath);
			await profileLoadHandle.Task;
			if (profileLoadHandle.Status == AsyncOperationStatus.Succeeded)
			{
				lipSync.profile = profileLoadHandle.Result;
			}
			else
			{
				Debug.LogError("Could not load lip sync profile addressable", this);
			}
			LipSyncBinder.BindLipSync(avatarReferences.Speaker);
		}
	}
}
