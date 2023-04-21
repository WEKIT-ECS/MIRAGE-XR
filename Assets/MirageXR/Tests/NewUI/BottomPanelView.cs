using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelView : BaseView
{
    private const float MOVE_DISTANSE = 300f;
    private const float ANIMATION_TIME = 0.5f;

    [SerializeField] private Toggle _btnHome;
    [SerializeField] private Toggle _btnProfile;
    [SerializeField] private Toggle _btnCreate;
    [SerializeField] private Toggle _btnSearch;
    [SerializeField] private Toggle _btnHelp;

    [Space]
    [Header("The colors of text and icons:")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightingColor;

    [Space]
    [SerializeField] private Image _iconHome;
    [SerializeField] private Image _iconProfile;
    [SerializeField] private Image _iconSearch;
    [SerializeField] private Image _iconHelp;

    [Space]
    [SerializeField] private TextMeshProUGUI _txtHome;
    [SerializeField] private TextMeshProUGUI _txtProfile;
    [SerializeField] private TextMeshProUGUI _txtSearch;
    [SerializeField] private TextMeshProUGUI _txtHelp;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    private Vector3 _startLocalPosition;
    private bool _isFirst = true;
    private bool _isVisible = true;

    public virtual void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _btnHome.onValueChanged.AddListener(OnHomeClicked);
        _btnProfile.onValueChanged.AddListener(OnProfileClicked);
        _btnCreate.onValueChanged.AddListener(OnCreateClicked);
        _btnSearch.onValueChanged.AddListener(OnSearchClicked);
        _btnHelp.onValueChanged.AddListener(OnHelpClicked);

        //_iconHome.color = highlightingColor; The switch selection is temporarily disabled. Need to update the bottom panel (maybe add a button for Steps?)
        //_txtHome.color = highlightingColor;
    }

    public void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        if (_isFirst)
        {
            _startLocalPosition = transform.localPosition;
            _isFirst = false;
        }

        transform.DOLocalMoveY(_startLocalPosition.y - MOVE_DISTANSE, ANIMATION_TIME);
        _isVisible = false;
    }

    public void Show()
    {
        if (_isVisible)
        {
            return;
        }

        transform.DOLocalMoveY(_startLocalPosition.y, ANIMATION_TIME);
        _isVisible = true;
    }

    public void SetHomeActive(bool value)
    {
        _btnCreate.gameObject.SetActive(value);
    }

    public void OnHomeClicked(bool isOn)
    {
        if (isOn)
        {
            //_iconHome.color = highlightingColor;
            //_txtHome.color = highlightingColor;
            rootView.ShowHomeView();
        }
        else
        {
            //_iconHome.color = normalColor;
            //_txtHome.color = normalColor;
        }
    }

    public void OnProfileClicked(bool isOn)
    {
        if (isOn)
        {
            //_iconProfile.color = highlightingColor;
            //_txtProfile.color = highlightingColor;
            rootView.ShowProfileView();
        }
        else
        {
            //_iconProfile.color = normalColor;
            //_txtProfile.color = normalColor;
        }
    }

    private void OnCreateClicked(bool isOn)
    {
        if (isOn)
        {
            rootView.CreateNewActivity();
        }
    }

    public void OnSearchClicked(bool isOn)
    {
        if (isOn)
        {
            //_iconSearch.color = highlightingColor;
            //_txtSearch.color = highlightingColor;
            rootView.ShowSearchView();
        }
        else
        {
            //_iconSearch.color = normalColor;
            //_txtSearch.color = normalColor;
        }
    }

    public void OnHelpClicked(bool isOn)
    {
        if (isOn)
        {
            //_iconHelp.color = highlightingColor;
            //_txtHelp.color = highlightingColor;
            rootView.ShowHelpView();
        }
        else
        {
            //_iconHelp.color = normalColor;
            //_txtHelp.color = normalColor;
        }
    }
}
