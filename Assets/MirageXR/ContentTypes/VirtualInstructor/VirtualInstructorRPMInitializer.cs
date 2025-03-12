using MirageXR.View;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MirageXR
{
	public class VirtualInstructorRPMInitializer : AvatarInitializer
    {
		// run this last
		public override int Priority => -100;

		public override void InitializeAvatar(GameObject avatar)
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

			avatar.AddComponent<Instructor>();
		}
	}
}
