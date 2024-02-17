using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace MirageXR
{
    public class WindowMovement : MonoBehaviour
    {
        [SerializeField] private ObjectManipulator objectManipulator;
        [SerializeField] private SpriteToggle toggleLock;

        public SpriteToggle SpriteToggle => toggleLock;

        public bool IsMoveable 
        {
            get => objectManipulator.enabled;
            set => objectManipulator.enabled = value;
        }

        private void Start()
        {
            IsMoveable = toggleLock.IsSelected;
        }

        private void OnEnable()
        {
            toggleLock.ValueChanged += OnValueChanged;
        }

        private void OnDisable()
        {
            toggleLock.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, bool value)
        {
            IsMoveable = value;
        }
    }
}