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

    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;
    private Vector3 _startLocalPosition;

    public virtual void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _rectTransform = (RectTransform)transform;
        _parentRectTransform = (RectTransform)_rectTransform.parent;
        _btnPrevious.onClick.AddListener(OnPreviousButtonClicked);
        _btnNext.onClick.AddListener(OnNextButtonClicked);
    }

    public void HideImmediate()
    {
        _rectTransform.Translate(0, _parentRectTransform.rect.height * -0.5f - MOVE_DISTANSE, 0);
    }

    public void Hide()
    {
        _rectTransform.DOLocalMoveY(_parentRectTransform.rect.height * -0.5f - MOVE_DISTANSE, ANIMATION_TIME);
    }

    public void Show()
    {
        _rectTransform.DOLocalMoveY(_parentRectTransform.rect.height * -0.5f, ANIMATION_TIME);
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
