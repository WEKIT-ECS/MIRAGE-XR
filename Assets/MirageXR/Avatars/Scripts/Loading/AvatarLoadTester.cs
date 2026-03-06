using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class AvatarLoadTester : MonoBehaviour
	{
		public AvatarLoader _avatarLoader;

		int index = 0;

        // Update is called once per frame
        private async void Update()
		{
			if (Input.GetKeyDown(KeyCode.F7))
			{
				if (index == 0)
				{
					await _avatarLoader.LoadAvatarAsync("DefaultAvatar");
				}
				else if (index == 1)
				{
					await _avatarLoader.LoadAvatarAsync("DocumentationAvatar");
				}
				else if (index == 2)
				{
					await _avatarLoader.LoadAvatarAsync("DocumentationAvatar2");
				}
				else if (index == 3)
				{
					await _avatarLoader.LoadAvatarAsync("FemaleDarkHairBlackClothing");
				}
				index = (index + 1) % 4;
			}
		}
	}
}
