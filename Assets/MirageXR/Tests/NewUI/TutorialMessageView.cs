using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _btnGotIt;

    private Action<TutorialModel> _action;
    private TutorialModel _model;

    public void Initialization(TutorialModel model, Action<TutorialModel> onButtonClicked)
    {
        _action = onButtonClicked;
        _model = model;
        _message.text = model.message;
        _btnGotIt.onClick.AddListener(OnGotItButtonClicked);
        var parentSize = ((RectTransform)transform.parent).rect.size;
        transform.localPosition = Vector3.zero; // new Vector3(parentSize.x * model.relativePosition.x, parentSize.y * model.relativePosition.y, 0);
    }

    private void OnGotItButtonClicked()
    {
        _action?.Invoke(_model);
        Destroy(gameObject);
    }
}