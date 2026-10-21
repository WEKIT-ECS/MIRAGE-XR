using UnityEngine;

namespace MirageXR
{
    public class Place : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;

        public Transform TargetTransform
        {
            get => _targetTransform;
            set => _targetTransform = value;
        }
    }
}
