using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BottomNavigationArrowsView : BaseView
{
    private const float MOVE_DISTANSE = 300f;
    private const float ANIMATION_TIME = 0.5f;

    [SerializeField] private Button _btnPrevious;
    [SerializeField] private Button _btnNext;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    private Vector3 _startLocalPosition;

    public virtual void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _btnPrevious.onClick.AddListener(OnPreviousButtonClicked);
        _btnNext.onClick.AddListener(OnNextButtonClicked);

        _startLocalPosition = transform.localPosition;
    }

    public void HideImmediate()
    {
        transform.Translate(0, -MOVE_DISTANSE, 0);
    }

    public void Hide()
    {
        transform.DOLocalMoveY(_startLocalPosition.y - MOVE_DISTANSE, ANIMATION_TIME);
    }

    public void Show()
    {
        transform.DOLocalMoveY(_startLocalPosition.y, ANIMATION_TIME);
    }

    private void OnPreviousButtonClicked()
    {
        rootView.activityView.ActivatePreviousAction();
    }

    private void OnNextButtonClicked()
    {
        rootView.activityView.ActivateNextAction();
    }
}
