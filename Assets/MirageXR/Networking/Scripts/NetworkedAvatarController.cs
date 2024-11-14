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

		private AvatarVisibilityController _avatarVisibilityController;
		public AvatarVisibilityController VisibilityController
		{
			get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarVisibilityController);
		}

		private void Start()
		{
			NetworkedUserData.NetworkedUserNameChanged += OnUserNameChanged;
			UpdateUserNameLabel();
		}

		private void OnDestroy()
		{
			NetworkedUserData.NetworkedUserNameChanged -= OnUserNameChanged;
		}

		private void OnUserNameChanged(string userName)
		{
			UpdateUserNameLabel();
		}

		private void UpdateUserNameLabel()
		{
			_nameLabel.text = _networkedUserData.UserName;
		}
	}
}
