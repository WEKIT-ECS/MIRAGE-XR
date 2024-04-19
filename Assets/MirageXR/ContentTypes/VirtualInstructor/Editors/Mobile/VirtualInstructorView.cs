using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VirtualInstructorView : PopupEditorBase
{
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    
    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    public override ContentType editorForType => ContentType.VIRTUALINSTRUCTOR; 
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
        
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);

        RootView_v2.Instance.HideBaseView();
    }
    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
    }

}
