using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedAvatarController : MonoBehaviour
	{
		private NetworkedAvatarReferences _avatarRefs;

		private async void Start()
		{
			_avatarRefs = GetComponent<NetworkedAvatarReferences>();
			_avatarRefs.UserData.NetworkedUserNameChanged += OnUserNameChanged;
			_avatarRefs.UserData.NetworkedAvatarUrlChanged += OnAvatarUrlChanged;
			UpdateUserNameLabel();
			await LoadAvatarAsync();
		}		

		private void OnDestroy()
		{
			_avatarRefs.UserData.NetworkedUserNameChanged -= OnUserNameChanged;
		}

		private void OnUserNameChanged(string userName)
		{
			UpdateUserNameLabel();
		}

		private async void OnAvatarUrlChanged(string newAvatarUrl)
		{
			Debug.LogTrace("Loading new avatar since avatar URL was changed to " + newAvatarUrl);
			await LoadAvatarAsync();
		}

		private async Task LoadAvatarAsync()
		{
			await _avatarRefs.OfflineReferences.Loader.LoadAvatarAsync(_avatarRefs.UserData.AvatarUrl);
		}

		private void UpdateUserNameLabel()
		{
			_avatarRefs.NameLabel.text = _avatarRefs.UserData.UserName;
		}
	}
}
