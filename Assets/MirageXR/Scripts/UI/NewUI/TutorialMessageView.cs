using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _btnGotIt;
    [SerializeField] private TMP_Text _btnText;
    [SerializeField] private Button _btnExit;
    [SerializeField] private TMP_Text _title;

    private Action<TutorialStepModelUI> _action;
    private TutorialStepModelUI _model;

    public void Initialization(TutorialStepModelUI model, Action<TutorialStepModelUI> onButtonClicked)
    {
        _action = onButtonClicked;
        _model = model;
        _message.text = model.Message;
        _title.text = model.ParentTutorial.Name;
        _btnGotIt.onClick.AddListener(OnGotItButtonClicked);
        _btnText.text = model.BtnText;
        transform.localPosition = new Vector3(0, GetPositionByY(), 0);
    }

    private float GetPositionByY()
    {
        var parentSize = ((RectTransform)transform.parent).rect.size;
        float k;
        switch (_model.Position)
        {
            case TutorialStepModelUI.MessagePosition.Top:
                k = 0.3f;
                break;
            case TutorialStepModelUI.MessagePosition.Middle:
                k = 0f;
                break;
            case TutorialStepModelUI.MessagePosition.Bottom:
                k = -0.3f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return parentSize.y * k;
    }

    private void OnGotItButtonClicked()
    {
        _action?.Invoke(_model);
        Destroy(gameObject);
    }
}