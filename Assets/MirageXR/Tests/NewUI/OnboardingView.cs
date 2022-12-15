using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnboardingView : UIBehaviour, ICanvasElement, IBeginDragHandler, IEndDragHandler, IDragHandler, ILayoutGroup
{
    public GameObject prefab;
    [SerializeField]
    protected RectTransform viewport;
    [SerializeField]
    protected RectTransform content;

    [SerializeField]
    private float snapTime = 0.3f;
    public bool swipeEnabled;

    private bool IsDragging { get; set; } //test

    private float _snapPosition;

    private float _velocity;

    private float _prevPosition;

    private float _viewportWidth;
    private float _contentWidth;

    private DrivenRectTransformTracker _drivenRectTracker;

    private float ScrollPosition
    {
        get => -content.anchoredPosition.x;
        set
        {
            var pos = content.anchoredPosition;
            pos.x = Mathf.Clamp(-value, -content.rect.size.x + viewport.rect.size.x, 0);
            content.anchoredPosition = pos;
        }
    }
    /// <summary>
    /// The number of tabs.
    /// </summary>
    private int Count => content.childCount;

    private int _selectedTabIndex = 0;

    protected override void OnEnable()
    {
        base.OnEnable();
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {
        if (executing == CanvasUpdate.PostLayout)
        {
            UpdateSelection();
        }
    }

    public virtual void LayoutComplete() { }

    public virtual void GraphicUpdateComplete() { }

    public void SetLayoutHorizontal()
    {
        _drivenRectTracker.Clear();
        var rect = viewport.rect;
        var w = rect.width;
        var h = rect.height;
        _drivenRectTracker.Add(
          this,
          content,
          DrivenTransformProperties.Anchors |
          DrivenTransformProperties.AnchoredPositionY |
          DrivenTransformProperties.SizeDelta);
        // Set the content size to match the viewport.
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.up;
        content.sizeDelta = new Vector2(content.childCount * w, 0);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
        for (var i = 0; i < content.childCount; i++)
        {
            var childRect = content.GetChild(i).GetComponent<RectTransform>();
            _drivenRectTracker.Add(
              this,
              childRect,
              DrivenTransformProperties.Anchors |
              DrivenTransformProperties.AnchoredPosition |
              DrivenTransformProperties.SizeDelta);
            childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, i * w, w);
            childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, h);
            childRect.localScale = Vector3.one;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void SetLayoutVertical() { }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (swipeEnabled)
        {
            ScrollPosition -= eventData.delta.x;
        }
        UpdateSelection();
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (Count > 0)
        {
            var w = viewport.rect.size.x;
            var predictedPos = ScrollPosition + _velocity * snapTime;
            _snapPosition = Mathf.Round(predictedPos / w) * w;
        }
        IsDragging = false;
    }

    protected virtual void LateUpdate()
    {
        var notYetUpdated = true;
        float newPosition;

        if (Mathf.Abs(_snapPosition - ScrollPosition) < 0.001f)
        {
            _velocity = 0f;
            newPosition = _snapPosition;
        }
        else
        {
            newPosition = Mathf.SmoothDamp(ScrollPosition, _snapPosition, ref _velocity, snapTime);
        }
        if (_prevPosition != newPosition)
        {
            _prevPosition = ScrollPosition = newPosition;
            UpdateSelection();
            notYetUpdated = false;
        }

        if (notYetUpdated && (_viewportWidth != viewport.rect.size.x || _contentWidth != content.rect.size.x))
        {
            UpdateSelection();
            _viewportWidth = viewport.rect.size.x;
            _contentWidth = content.rect.size.x;
        }
    }

    private void UpdateSelection()
    {
        // Do something
    }

    public void SkipBtnClicked()
    {
        prefab.SetActive(false); // temp
    }

    public void NextBtnClicked()
    {
        _selectedTabIndex += 1;
        _snapPosition = viewport.rect.width * _selectedTabIndex;
    }

    public void BackBtnClicked()
    {
        _selectedTabIndex -= 1;
        _snapPosition = viewport.rect.width * _selectedTabIndex;
    }

    public void StartEditingTutorialClicked()
    {

    }

    public void StartViewingTutorialClicked()
    {

    }
}





