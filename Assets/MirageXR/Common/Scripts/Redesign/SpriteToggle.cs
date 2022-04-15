using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SpriteToggle : MonoBehaviour
    {
        [SerializeField] private Sprite _unselectedIcon;
        [SerializeField] private Sprite _selectedIcon;

        [SerializeField] private Image _iconImage;

        [SerializeField] private bool _isSelected;

        public event EventHandler<bool> ValueChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                bool changed = _isSelected != value;
                _isSelected = value;
                UpdateDisplay();
                if (changed)
                {
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        private void Awake()
        {
            // update display in the beginning to show the initial value
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_iconImage)
            {
                if (_isSelected)
                {
                    _iconImage.sprite = _selectedIcon;
                }
                else
                {
                    _iconImage.sprite = _unselectedIcon;
                }
            }

        }

        public void ToggleValue()
        {
            IsSelected = !IsSelected;
        }


    }
}