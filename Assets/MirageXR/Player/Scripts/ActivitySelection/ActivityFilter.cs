using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityFilter : MonoBehaviour
    {
        [SerializeField] private InputField _inputField;
        [SerializeField] private SessionListView _sessionListView;


        private void OnEnable()
        {
                _inputField.onValueChanged.AddListener(OnTextUpdated);
        }


        private void OnTextUpdated(string value)
        {
            Debug.Log($"Text updated: {value}");
            if (string.IsNullOrEmpty(value))
            {
                _sessionListView.SetAllItems();
            }
            else
            {
                string lowerValue = value.ToLower();

                _sessionListView.SetSearchedItems(_sessionListView.AllItems.FindAll(
                    item => item.Name.ToLower().Contains(lowerValue)));
            }
        }
    }
}