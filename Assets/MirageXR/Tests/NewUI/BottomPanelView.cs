using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelView : BaseView
{
    private const float MOVE_DISTANSE = 300f;
    private const float ANIMATION_TIME = 0.5f;

    [SerializeField] private Toggle _toggleHome;
    [SerializeField] private Button _btnProfile;
    [SerializeField] private Button _btnCreate;
    [SerializeField] private Button _btnSearch;
    [SerializeField] private Button _btnHelp;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    private Vector3 _startLocalPosition;

    public virtual void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _toggleHome.onValueChanged.AddListener(OnHomeValueChanged);
        _btnProfile.onClick.AddListener(OnProfileClicked);
        _btnCreate.onClick.AddListener(OnCreateClicked);
        _btnSearch.onClick.AddListener(OnSearchClicked);
        _btnHelp.onClick.AddListener(OnHelpClicked);

        _startLocalPosition = transform.localPosition;
    }

    public void Hide()
    {
        transform.DOLocalMoveY(_startLocalPosition.y - MOVE_DISTANSE, ANIMATION_TIME);
    }

    public void Show()
    {
        transform.DOLocalMoveY(_startLocalPosition.y, ANIMATION_TIME);
    }

    public void SetHomeActive(bool value)
    {
        _toggleHome.isOn = value;
        _btnCreate.gameObject.SetActive(value);
    }

    private void OnHomeValueChanged(bool value)
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
