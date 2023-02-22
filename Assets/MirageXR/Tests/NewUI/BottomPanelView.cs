using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelView : BaseView
{
    private const float MOVE_DISTANSE = 300f;
    private const float ANIMATION_TIME = 0.5f;

    [SerializeField] private Button _btnHome;
    [SerializeField] private Button _btnProfile;
    [SerializeField] private Button _btnCreate;
    [SerializeField] private Button _btnSearch;
    [SerializeField] private Button _btnHelp;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    private Vector3 _startLocalPosition;
    private bool _isFirst = true;
    private bool _isVisible = true;

    public virtual void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _btnHome.onClick.AddListener(OnHomeClicked);
        _btnProfile.onClick.AddListener(OnProfileClicked);
        _btnCreate.onClick.AddListener(OnCreateClicked);
        _btnSearch.onClick.AddListener(OnSearchClicked);
        _btnHelp.onClick.AddListener(OnHelpClicked);
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

    private void OnHomeClicked()
    {
        rootView.ShowHomeView();
    }

    private void OnProfileClicked()
    {
        rootView.ShowProfileView();
    }

    private void OnCreateClicked()
    {
        rootView.CreateNewActivity();
    }

    private void OnSearchClicked()
    {
        rootView.ShowSearchView();
    }

    private void OnHelpClicked()
    {
        rootView.ShowHelpView();
    }
}
