using ReadyPlayerMe.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class AvatarEyeInitializer : AvatarInitializer
    {
		public override int Priority => 0;

		[SerializeField] private float _blinkInterval = 5f;

		public override void InitializeAvatar(GameObject avatar)
		{
			EyeAnimationHandler eyeAnimation = avatar.AddComponent<EyeAnimationHandler>();
			eyeAnimation.BlinkInterval = _blinkInterval;
		}
	}
}
