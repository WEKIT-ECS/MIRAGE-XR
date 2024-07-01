using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _btnGotIt;
    [SerializeField] private TMP_Text _btnText;

    private Action<TutorialModelUI> _action;
    private TutorialModelUI _model;

    public void Initialization(TutorialModelUI model, Action<TutorialModelUI> onButtonClicked)
    {
        _action = onButtonClicked;
        _model = model;
        _message.text = model.Message;
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
            case TutorialModelUI.MessagePosition.Top:
                k = 0.3f;
                break;
            case TutorialModelUI.MessagePosition.Middle:
                k = 0f;
                break;
            case TutorialModelUI.MessagePosition.Bottom:
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