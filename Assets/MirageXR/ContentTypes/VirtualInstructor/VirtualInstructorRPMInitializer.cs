using Cysharp.Threading.Tasks;
using MirageXR.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace MirageXR
{
	public class VirtualInstructorRPMInitializer : AvatarInitializer
	{
		// run this last
		public override int Priority => -100;

		public override void InitializeAvatar(GameObject avatar)
		{
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
		}
	}
}
