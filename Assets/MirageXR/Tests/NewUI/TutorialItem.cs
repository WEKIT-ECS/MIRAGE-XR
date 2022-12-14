using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialItem : MonoBehaviour
{
    private const float THRESHOLD_CHANGE = 0.05f;

    [SerializeField] private string _id;
    [SerializeField] private GameObject _interactableObject;
    [SerializeField] private bool _isPartOfScrollView;

    private Vector3 _lastTraceablePosition;
    private bool _isTrackingActivated;
    private Transform _traceable;

    public string id => _id;

    public Button button => _interactableObject.GetComponent<Button>();

    public Toggle toggle => _interactableObject.GetComponent<Toggle>();

    public TMP_InputField inputField => _interactableObject.GetComponent<TMP_InputField>();

    public bool isPartOfScrollView => _isPartOfScrollView;

    private void Update()
    {
        if (!_isTrackingActivated)
        {
            return;
        }

        var diffVector = _traceable.position - _lastTraceablePosition;
        if (diffVector.magnitude >= THRESHOLD_CHANGE)
        {
            _lastTraceablePosition = transform.position;
            OnTraceablePositionChanged();
        }
    }

    private void OnTraceablePositionChanged()
    {
        transform.position = _traceable.position;
    }

    public void StartTracking(Transform traceable)
    {
        _isTrackingActivated = true;
        _traceable = traceable;
        _lastTraceablePosition = transform.position;
    }

    public void StopTracking()
    {
        _isTrackingActivated = false;
        _traceable = null;
    }

    public void ScrollToTop()
    {
        if (_isPartOfScrollView)
        {
            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
