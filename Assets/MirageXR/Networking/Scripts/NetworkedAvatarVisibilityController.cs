using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class NetworkedAvatarVisibilityController : NetworkBehaviour
    {
		private AvatarVisibilityController _avatarVisibilityController;

		public AvatarVisibilityController VisibilityController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarVisibilityController);
		}

		public override void Spawned()
		{
			base.Spawned();

			if (HasStateAuthority)
			{
				VisibilityController.FadeVisibility = false;
				VisibilityController.Visible = false;
			}
		}
	}
}
