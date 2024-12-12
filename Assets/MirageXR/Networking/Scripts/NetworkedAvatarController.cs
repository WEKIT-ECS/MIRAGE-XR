using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedAvatarController : MonoBehaviour
	{
		private NetworkedAvatarReferences _avatarRefs;

		private void Start()
		{
			_avatarRefs = GetComponent<NetworkedAvatarReferences>();
			_avatarRefs.UserData.NetworkedUserNameChanged += OnUserNameChanged;
			_avatarRefs.UserData.NetworkedAvatarUrlChanged += OnAvatarUrlChanged;
			UpdateUserNameLabel();
			LoadAvatar();
		}		

		private void OnDestroy()
		{
			_avatarRefs.UserData.NetworkedUserNameChanged -= OnUserNameChanged;
		}

		private void OnUserNameChanged(string userName)
		{
			UpdateUserNameLabel();
		}

		private void OnAvatarUrlChanged(string newAvatarUrl)
		{
			Debug.LogTrace("Loading new avatar since avatar URL was changed to " + newAvatarUrl);
			LoadAvatar();
		}

		private void LoadAvatar()
		{
			_avatarRefs.OfflineReferences.Loader.LoadAvatar(_avatarRefs.UserData.AvatarUrl);
		}

		private void UpdateUserNameLabel()
		{
			_avatarRefs.NameLabel.text = _avatarRefs.UserData.UserName;
		}
	}
}
