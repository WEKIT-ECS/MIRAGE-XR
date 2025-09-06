using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class CollapsableContainer : MonoBehaviour
    {
        [SerializeField] private Button _collapseButton;
        [SerializeField] private GameObject _expandedView;
        [Tooltip("A collapsed view is optional")]
		[SerializeField] private GameObject _collapsedView;

		[SerializeField] private bool _isExpanded;

		public bool IsExpanded
		{
			get => _isExpanded; set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					UpdateView();
				}
			}
		}

		private void OnEnable()
		{
			_collapseButton.onClick.AddListener(CollapseButtonClicked);
			UpdateView();
		}

		private void OnDisable()
		{
			_collapseButton.onClick.RemoveListener(CollapseButtonClicked);
		}

		private void CollapseButtonClicked()
		{
			IsExpanded = !IsExpanded;
		}

		private void UpdateView()
		{
			float buttonFlipRotation = _isExpanded ? 180f : 0f;
			_collapseButton.transform.localRotation = Quaternion.Euler(0, 0, buttonFlipRotation);
			_expandedView.SetActive(_isExpanded);
			if (_collapsedView)
			{
				_collapsedView.SetActive(!_isExpanded);
			}
		}
	}
}
