using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirageXR
{
    public class AvatarManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameLabel;

        private NetworkedUserData _networkedUserData;

		private void Awake()
		{
			_networkedUserData = GetComponent<NetworkedUserData>();
		}

		private void Start()
		{
			_networkedUserData.NetworkedUserNameChanged += OnUserNameChanged;
			UpdateUserNameLabel();
		}

		private void OnDestroy()
		{
			_networkedUserData.NetworkedUserNameChanged -= OnUserNameChanged;
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
