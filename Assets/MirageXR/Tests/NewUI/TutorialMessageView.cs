using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _btnGotIt;
    [SerializeField] private TMP_Text _btnText;

    private Action<TutorialModel> _action;
    private TutorialModel _model;

    public void Initialization(TutorialModel model, Action<TutorialModel> onButtonClicked)
    {
        _action = onButtonClicked;
        _model = model;
        _message.text = model.message;
        _btnGotIt.onClick.AddListener(OnGotItButtonClicked);
        _btnText.text = model.btnText;
        transform.localPosition = new Vector3(0, GetPositionByY(), 0);
    }

    private float GetPositionByY()
    {
        var parentSize = ((RectTransform)transform.parent).rect.size;
        float k;
        switch (_model.position)
        {
            case TutorialModel.MessagePosition.Top:
                k = 0.3f;
                break;
            case TutorialModel.MessagePosition.Middle:
                k = 0f;
                break;
            case TutorialModel.MessagePosition.Bottom:
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