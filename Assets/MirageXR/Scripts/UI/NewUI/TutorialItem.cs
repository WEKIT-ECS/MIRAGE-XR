using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItem : MonoBehaviour
{
    private const float THRESHOLD_CHANGE = 0.05f;

    [SerializeField] private bool _getIdFromName;
    [SerializeField] private string _id;
    [SerializeField] private GameObject _interactableObject;
    [SerializeField] private bool _isPartOfScrollView;
    [SerializeField] private float _delay;

    private Vector3 _lastTraceablePosition;
    private bool _isTrackingActivated;
    private Transform _traceable;

    public string Id => _getIdFromName ? name : _id;
    public Button Button => _interactableObject.GetComponentInChildren<Button>();

    public Toggle Toggle => _interactableObject.GetComponent<Toggle>();

    public TMP_InputField InputField => _interactableObject.GetComponent<TMP_InputField>();

    public bool IsPartOfScrollView => _isPartOfScrollView;

    public float Delay => _delay;

    public void SetId(string id)
    {
        this._id = id;
    }

    public void SetInteractableObject(GameObject gameObject)
    {
        this._interactableObject = gameObject;
    }

    public void SetDelay(float delay)
    {
        this._delay = delay;
    }

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
