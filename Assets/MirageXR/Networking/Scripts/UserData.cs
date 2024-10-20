using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class UserData
    {
        private string _userName;

        public string UserName
        {
            get => _userName; set
            {
                _userName = value;
                UserNameChanged?.Invoke(value);
            }
        }
        public event Action<string> UserNameChanged;
    }
}
