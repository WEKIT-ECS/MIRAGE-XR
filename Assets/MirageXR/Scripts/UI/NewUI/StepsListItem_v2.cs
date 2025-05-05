using LearningExperienceEngine;
using System;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;
using Step = LearningExperienceEngine.Action;

public class StepsListItem_v2 : MonoBehaviour
{
    private static ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private TMP_Text _txtNumber;
    [SerializeField] private TMP_Text _txtStepName;
    [SerializeField] private TMP_Text _txtStepDescription;
    [SerializeField] private Button _btnStep;
    [SerializeField] private Button _btnEditButton;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private Button _btnImageMarkerPopup;
    [SerializeField] private GameObject _moveIcon;
    [SerializeField] private GameObject _stepStatus;
    [SerializeField] private GameObject _stepDoneImage;
    [SerializeField] private GameObject _stepCurrentImage;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private ImageMarkerPopup _imageMarkerPopup;
    [SerializeField] private GameObject _stepSelected;

    private ActivityStep _step;
    private int _number;
    private Action<ActivityStep> _onStepClick;
    private Action<ActivityStep> _onEditClick;
    private Action<ActivityStep, Action> _onDeleteClick;
    private Action<ActivityStep, int, int> _onSiblingIndexChanged;
    private string _imageMarkerUrl;

    public ActivityStep step => _step;

    public void Init(Action<ActivityStep> onStepClick, Action<ActivityStep> onEditClick, Action<ActivityStep, Action> onDeleteClick, Action<ActivityStep, int, int> onSiblingIndexChanged)
    {
        _onStepClick = onStepClick;
        _onEditClick = onEditClick;
        _onDeleteClick = onDeleteClick;
        _onSiblingIndexChanged = onSiblingIndexChanged;
        _btnStep.onClick.AddListener(OnStepClick);
        _btnDelete.onClick.AddListener(OnDeleteClick);
        _btnEditButton.onClick.AddListener(OnEditClick);
        _btnImageMarkerPopup.onClick.AddListener(OnImageMarkerButtonClick);
        _dragAndDropController.onSiblingIndexChanged.AddListener(OnSiblingIndexChanged);
        OnEditModeChanged(activityManager.EditModeActive);

        //LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        //LearningExperienceEngine.EventManager.OnActionModified += OnActionModified;
    }

    public void UpdateView(ActivityStep step, int number)
    {
        _step = step;
        _number = number;

        _txtStepName.text = _step.Name;
        _txtNumber.text = (_number + 1).ToString("00");
        if (RootObject.Instance.LEE.StepManager.CurrentStep != null)
        {
            var isCurrent = step.Id == RootObject.Instance.LEE.StepManager.CurrentStep.Id;
            _stepCurrentImage.SetActive(isCurrent);
            _stepSelected.SetActive(isCurrent);
        }
        //_stepDoneImage.SetActive(_step.isCompleted && !isCurrent);

        //_btnImageMarkerPopup.gameObject.SetActive(ImageMarkerCheck());
    }

    /*private void OnActionModified(Step step)
    {
        if (step == _step)
        {
            _txtStepName.text = step.instruction.title;
            _txtStepDescription.text = step.instruction.description;
        }
    }*/

    public void OnEditModeChanged(bool value)
    {
        _btnDelete.gameObject.SetActive(value);
        _stepStatus.SetActive(!value);
        _btnEditButton.gameObject.SetActive(value);
        _moveIcon.gameObject.SetActive(false);
        //_moveIcon.gameObject.SetActive(value);
    }

    private void OnStepClick()
    {
        _onStepClick(_step);
    }

    private void OnSiblingIndexChanged(int oldIndex, int newIndex)
    {
        _onSiblingIndexChanged(_step, oldIndex, newIndex);
    }

    private void OnDeleteClick()
    {
        _onDeleteClick(_step, null);
    }

    public void OnEditClick()
    {
        _onEditClick(_step);
        MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActionAugmentations);
    }

    public void OnImageMarkerButtonClick()
    {
        var popup = (ImageMarkerPopup)PopupsViewer.Instance.Show(_imageMarkerPopup);
        popup.SetImage(_imageMarkerUrl);
    }

    /*private bool ImageMarkerCheck()
    {
        bool imageMarker = false;

        var augmentations = _step.enter.activates;

        foreach (var augmentation in augmentations)
        {
            if (augmentation.predicate == "imagemarker")
            {
                imageMarker = true;
                _imageMarkerUrl = augmentation.url;
            }
        }

        return imageMarker;
    }*/
}
