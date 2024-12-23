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
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.F7))
			{
				if (index == 0)
				{
					_avatarLoader.LoadAvatar("https://models.readyplayer.me/66edca96b26e13258a79831a.glb");
				}
				else if (index == 1)
				{
					_avatarLoader.LoadAvatar("https://models.readyplayer.me/667d532bf636bee47d5e7146.glb");
				}
				else if (index == 2)
				{
					_avatarLoader.LoadAvatar("https://models.readyplayer.me/673a4113c99a8ebbd293724f.glb");
				}
				else if (index == 3)
				{
					_avatarLoader.LoadAvatar("https://models.readyplayer.me/670645c0c8bf0b5a9f60d204.glb");
				}
				index = (index + 1) % 4;
			}
		}
	}
}
