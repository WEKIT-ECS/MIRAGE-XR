using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialItem : MonoBehaviour
{
    private const float THRESHOLD_CHANGE = 0.05f;

    public string Id;
    public GameObject InteractableObject;
    public bool IsPartOfScrollView;

    private Vector3 _lastTraceablePosition;
    private bool _isTrackingActivated;
    private Transform _traceable;

    public Button button => InteractableObject.GetComponent<Button>();

    public Toggle toggle => InteractableObject.GetComponent<Toggle>();

    public TMP_InputField inputField => InteractableObject.GetComponent<TMP_InputField>();

    public bool isPartOfScrollView => IsPartOfScrollView;

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
        if (IsPartOfScrollView)
        {
            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
