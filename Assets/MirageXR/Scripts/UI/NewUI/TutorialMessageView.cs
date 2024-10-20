using MirageXR;
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

    private Action<TutorialStepModelUI> _nextAction;
    private Action<TutorialStepModelUI> _exitAction;
    private TutorialStepModelUI _model;

    public void Initialization(TutorialStepModelUI model, Action<TutorialStepModelUI> onNextClicked, Action<TutorialStepModelUI> onExitClicked)
    {
        _nextAction = onNextClicked;
        _exitAction = onExitClicked;
        _model = model;
        _message.text = model.Message;
        _btnGotIt.onClick.AddListener(OnGotItButtonClicked);
        _btnExit.onClick.AddListener(OnExitButtonClicked);
        _btnText.text = model.BtnText;
        transform.localPosition = new Vector3(0, GetPositionByY(), 0);

        if (model.CanGoNext)
        {
            GameObject parentObject = _btnGotIt.transform.parent.gameObject;
            parentObject.SetActive(true);
        }

        if (model.ParentTutorial != null)
        {
            // Context help has no parent and no exit button and title...
            _title.text = model.ParentTutorial.Name;
            _title.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            // but it has the GotIt button.
            GameObject parentObject = _btnGotIt.transform.parent.gameObject;
            parentObject.SetActive(true);
        }
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
        _nextAction?.Invoke(_model);
        Destroy(gameObject);
    }

    private void OnExitButtonClicked()
    {
        _exitAction?.Invoke(_model);
        Destroy(gameObject);
    }
}
