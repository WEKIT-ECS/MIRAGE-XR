using LearningExperienceEngine;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepsListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _txtNumber;
    [SerializeField] private TMP_Text _txtStepName;
    [SerializeField] private Button _btnStep;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private GameObject _stepStatus;
    [SerializeField] private GameObject _stepDoneImage;
    [SerializeField] private GameObject _stepCurrentImage;

    private LearningExperienceEngine.Action _step;
    private int _number;
    private System.Action<Action> _onStepClick;
    private System.Action<Action> _onDeleteClick;

    public void Init(System.Action<Action> onStepClick, System.Action<Action> onDeleteClick)
    {
        _onStepClick = onStepClick;
        _onDeleteClick = onDeleteClick;
        _btnStep.onClick.AddListener(OnStepClick);
        _btnDelete.onClick.AddListener(OnDeleteClick);
    }

    public void UpdateView(Action step, int number)
    {
        _step = step;
        _number = number;
        _txtStepName.text = _step.instruction.title;
        _txtNumber.text = _number.ToString("00");
        var isCurrent = _step.id == LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActiveActionId;
        _stepCurrentImage.SetActive(isCurrent);
        _stepDoneImage.SetActive(_step.isCompleted && !isCurrent);
    }

    public void OnEditModeChanged(bool value)
    {
        _btnDelete.gameObject.SetActive(value);
        _stepStatus.SetActive(!value);
    }

    private void OnStepClick()
    {
        _onStepClick(_step);
    }

    private void OnDeleteClick()
    {
        _onDeleteClick(_step);
    }
}
