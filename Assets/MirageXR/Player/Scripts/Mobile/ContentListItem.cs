using LearningExperienceEngine;
using System.Linq;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentListItem : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    [SerializeField] private TMP_Text _txtType;
    [SerializeField] private TMP_Text _txtFrom;
    [SerializeField] private TMP_Text _txtTo;
    [SerializeField] private Image _imgType;
    [SerializeField] private Button _btnContent;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private Button _btnMinusFrom;
    [SerializeField] private Button _btnPlusFrom;
    [SerializeField] private Button _btnMinusTo;
    [SerializeField] private Button _btnPlusTo;
    [SerializeField] private Button _btnNavigator;
    [SerializeField] private Image _imageNavigatorCheck;

    private ContentListView _parentView;
    private LearningExperienceEngine.ToggleObject _content;
    private LearningExperienceEngine.ContentType _type;
    private int _from;
    private int _to;

    private int _maxStepIndex => activityManager.ActionsOfTypeAction.Count - 1;

    public void Init(ContentListView parentView)
    {
        _parentView = parentView;
        _btnNavigator.onClick.AddListener(OnNavigatorClick);
        _btnContent.onClick.AddListener(OnContentClick);
        _btnDelete.onClick.AddListener(OnDeleteClick);
        _btnMinusFrom.onClick.AddListener(OnMinusFromClick);
        _btnPlusFrom.onClick.AddListener(OnPlusFromClick);
        _btnMinusTo.onClick.AddListener(OnMinusToClick);
        _btnPlusTo.onClick.AddListener(OnPlusToClick);
    }

    public void UpdateView(LearningExperienceEngine.ToggleObject content)
    {
        _content = content;
        _type = LearningExperienceEngine.ContentTypeExtension.ParsePredicate(_content.predicate);
        _txtType.text = _content.predicate;
        _imgType.sprite = _type.GetIcon();

        var stepList = activityManager.ActionsOfTypeAction;

        var startStep = stepList.FindIndex(step => step.enter.activates.Any(t => t.poi == _content.poi));
        var lastStep = stepList.FindLastIndex(step => step.enter.activates.Any(t => t.poi == _content.poi));

        _from = startStep;
        _to = lastStep;
        _txtFrom.text = (_from + 1).ToString();
        _txtTo.text = (_to + 1).ToString();

        _imageNavigatorCheck.enabled = false;
        if (_parentView.navigatorId == _content.poi)
        {
            _imageNavigatorCheck.enabled = true;
            TaskStationDetailMenu.Instance.NavigatorTarget = ActionListMenu.CorrectTargetObject(_content);
        }
    }

    private void OnNavigatorClick()
    {
        _parentView.navigatorId = _parentView.navigatorId != _content.poi ? _content.poi : null;
    }

    public void OnEditModeChanged(bool value)
    {
        _btnDelete.gameObject.SetActive(value);
        _btnMinusFrom.interactable = value;
        _btnPlusFrom.interactable = value;
        _btnMinusTo.interactable = value;
        _btnPlusTo.interactable = value;
    }

    private void OnContentClick()
    {
        var type = LearningExperienceEngine.ContentTypeExtension.ParsePredicate(_content.predicate);
        var editor = _parentView.editors.FirstOrDefault(t => t.editorForType == type);
        if (editor == null)
        {
            Debug.LogError($"there is no editor for the type {type}");
            return;
        }
        PopupsViewer.Instance.Show(editor, _parentView.currentStep, _content);
    }

    private void OnDeleteClick()
    {
        LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.DeleteAugmentation(_content);
        if (_parentView.navigatorId == _content.poi)
        {
            TaskStationDetailMenu.Instance.NavigatorTarget = null;
        }
    }

    private void OnMinusFromClick()
    {
        if (_from == 0) return;
        _from--;
        _txtFrom.text = (_from + 1).ToString();
        UpdateStep();
    }

    private void OnPlusFromClick()
    {
        if (_from == _to) return;
        _from++;
        _txtFrom.text = (_from + 1).ToString();
        UpdateStep();
    }

    private void OnMinusToClick()
    {
        if (_to == _from) return;
        _to--;
        _txtTo.text = (_to + 1).ToString();
        UpdateStep();
    }

    private void OnPlusToClick()
    {
        if (_to == _maxStepIndex) return;
        _to++;
        _txtTo.text = (_to + 1).ToString();
        UpdateStep();
    }

    private void UpdateStep()
    {
        LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAllAugmentationsBetweenSteps(_from, _to, _content, Vector3.zero);
        if (_type == LearningExperienceEngine.ContentType.CHARACTER)
        {
            activityManager.SaveData();
        }
    }
}