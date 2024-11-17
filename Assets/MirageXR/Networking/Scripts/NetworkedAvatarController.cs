using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirageXR
{
	public class NetworkedAvatarController : MonoBehaviour
	{
		[SerializeField] private TMP_Text _nameLabel;


		private NetworkedUserData _networkedUserData;

		public NetworkedUserData NetworkedUserData
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _networkedUserData);
		}

		private AvatarLoader _avatarLoader;
		public AvatarLoader AvatarLoader
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarLoader);
		}

		private AvatarVisibilityController _avatarVisibilityController;
		public AvatarVisibilityController VisibilityController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarVisibilityController);
		}

		private void Start()
		{
			NetworkedUserData.NetworkedUserNameChanged += OnUserNameChanged;
			NetworkedUserData.NetworkedAvatarUrlChanged += OnAvatarUrlChanged;
			UpdateUserNameLabel();
			LoadAvatar();
		}		

		private void OnDestroy()
		{
			NetworkedUserData.NetworkedUserNameChanged -= OnUserNameChanged;
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
			AvatarLoader.LoadAvatar(_networkedUserData.AvatarUrl);
		}

		private void UpdateUserNameLabel()
		{
			_nameLabel.text = _networkedUserData.UserName;
		}
	}
}
