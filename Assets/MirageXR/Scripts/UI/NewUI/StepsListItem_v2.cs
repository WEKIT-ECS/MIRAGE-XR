using LearningExperienceEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private Button _btnLock;
    [SerializeField] private Image _imgLock;
    [SerializeField] private Sprite _spriteLock;
    [SerializeField] private Sprite _spriteUnlock;
    [SerializeField] private Button _btnImageMarkerPopup;
    [SerializeField] private GameObject _moveIcon;
    [SerializeField] private GameObject _stepStatus;
    [SerializeField] private GameObject _stepDoneImage;
    [SerializeField] private GameObject _stepCurrentImage;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private ImageMarkerPopup _imageMarkerPopup;
    [SerializeField] private GameObject _stepSelected;

    private static readonly HashSet<string> _completedStepIds = new HashSet<string>();

    private ActivityStep _step;
    private int _number;
    private Action<ActivityStep> _onStepClick;
    private Action<ActivityStep> _onEditClick;
    private Action<ActivityStep, Action> _onDeleteClick;
    private Action<ActivityStep, int, int> _onSiblingIndexChanged;
    private string _imageMarkerUrl;

    public ActivityStep step => _step;
    public bool IsCompleted => _step != null && _completedStepIds.Contains(_step.Id.ToString());

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

        if (_btnLock != null)
        {
            _btnLock.onClick.RemoveAllListeners();
            _btnLock.onClick.AddListener(ToggleLock);
        }

        if (_stepStatus != null)
        {
            var btnStatus = _stepStatus.GetComponent<Button>();
            if (btnStatus == null)
            {
                btnStatus = _stepStatus.AddComponent<Button>();
            }

            var statusImg = _stepStatus.GetComponent<Image>();
            if (statusImg == null)
            {
                statusImg = _stepStatus.AddComponent<Image>();
                statusImg.color = Color.clear;
            }
            statusImg.raycastTarget = true;

            btnStatus.onClick.RemoveAllListeners();
            btnStatus.onClick.AddListener(OnStepStatusClicked);
        }

        OnEditModeChanged(activityManager.EditModeActive);

        //LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        //LearningExperienceEngine.EventManager.OnActionModified += OnActionModified;
    }

    private void OnDestroy()
    {
        if (_btnLock != null)
        {
            _btnLock.onClick.RemoveListener(ToggleLock);
        }

        if (_stepStatus != null)
        {
            var btnStatus = _stepStatus.GetComponent<Button>();
            if (btnStatus != null)
            {
                btnStatus.onClick.RemoveListener(OnStepStatusClicked);
            }
        }
    }

    public void UpdateView(ActivityStep step, int number)
    {
        _step = step;
        _number = number;

        var title = string.IsNullOrWhiteSpace(_step?.Name) ? $"Step {number + 1}" : _step.Name;
        _txtStepName.text = title;
        _txtStepName.enableAutoSizing = true;
        _txtStepName.fontSizeMin = 14;
        _txtStepName.fontSizeMax = 32;
        _txtStepName.overflowMode = TextOverflowModes.Ellipsis;
        _txtStepName.enableWordWrapping = true;

        _txtNumber.text = (_number + 1).ToString("00");

        if (_txtStepDescription != null)
        {
            _txtStepDescription.text = _step?.Description ?? string.Empty;
            _txtStepDescription.enableWordWrapping = true;
            _txtStepDescription.overflowMode = TextOverflowModes.Overflow;
        }

        bool isCurrent = false;
        if (RootObject.Instance.LEE.StepManager.CurrentStep != null)
        {
            isCurrent = step.Id == RootObject.Instance.LEE.StepManager.CurrentStep.Id;
        }

        // Task 7: Remove radio button current step dot from view mode
        if (_stepCurrentImage != null)
        {
            _stepCurrentImage.SetActive(false);
        }

        // Task 7: Replace with theme background color highlighting (#3D4582)
        UpdateSelectionVisual(isCurrent);

        // Task 6 & 8: Update completed checkmark visual
        UpdateCompletedVisual(IsCompleted);

        UpdateLockVisual();
    }

    private void UpdateSelectionVisual(bool isCurrent)
    {
        if (_stepSelected != null)
        {
            var selectedImg = _stepSelected.GetComponent<Image>();
            if (selectedImg != null)
            {
                selectedImg.color = new Color(0.2392157f, 0.2705882f, 0.509804f, 1f); // #3D4582
            }
            _stepSelected.transform.SetAsFirstSibling();
            _stepSelected.SetActive(isCurrent);
        }
    }

    private void UpdateCompletedVisual(bool isCompleted)
    {
        if (_stepDoneImage != null)
        {
            _stepDoneImage.SetActive(isCompleted);
        }

        if (_stepStatus != null)
        {
            var emptyTransform = _stepStatus.transform.Find("Empty");
            if (emptyTransform != null && emptyTransform.TryGetComponent<Image>(out var emptyImg))
            {
                emptyImg.color = isCompleted ? new Color(0.29803923f, 0.6862745f, 0.3137255f, 1f) : Color.white;
            }
        }
    }

    public void ToggleLock()
    {
        if (_step == null) return;
        if (_step.Location == null)
        {
            _step.Location = new Location();
        }

        _step.Location.IsLocked = !_step.Location.IsLocked;

        // If this is the active step, update StepView in the scene
        var currentStep = RootObject.Instance.LEE.StepManager.CurrentStep;
        if (currentStep != null && _step.Id == currentStep.Id)
        {
            var stepView = RootObject.Instance.ViewManager.ActivityView.StepView;
            if (stepView != null)
            {
                stepView.SetLocked(_step.Location.IsLocked, cascadeToContents: true);
            }
        }

        RootObject.Instance.LEE.StepManager.UpdateStep(_step);

        // Cascade lock state to all contents belonging to this step
        var allContents = RootObject.Instance.LEE.ContentManager.GetContents();
        if (allContents != null)
        {
            var stepContents = new List<Content>();
            foreach (var content in allContents)
            {
                if (content.Steps != null && content.Steps.Contains(_step.Id))
                {
                    if (content.Location == null)
                    {
                        content.Location = new Location();
                    }
                    content.Location.IsLocked = _step.Location.IsLocked;
                    stepContents.Add(content);
                }
            }

            if (stepContents.Count > 0)
            {
                RootObject.Instance.LEE.ContentManager.UpdateContents(stepContents);
            }
        }

        UpdateLockVisual();
    }

    private void UpdateLockVisual()
    {
        if (_imgLock != null)
        {
            bool isLocked = _step?.Location?.IsLocked ?? false;
            _imgLock.sprite = isLocked ? _spriteLock : _spriteUnlock;
        }
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
        _stepStatus.SetActive(!value); // Checkbox visible in view mode (both vertical and horizontal)
        _btnEditButton.gameObject.SetActive(value);
        if (_btnLock != null)
        {
            _btnLock.gameObject.SetActive(value);
        }
        _moveIcon.gameObject.SetActive(false);
    }

    private void OnStepStatusClicked()
    {
        CheckStepCompletedAsync().Forget();
    }

    private async UniTask CheckStepCompletedAsync()
    {
        if (_step == null) return;

        var dialog = RootView_v2.Instance?.dialog ?? MenuManager.Instance?.Dialog;
        bool isCancelled = false;

        // 1. Show instruction dialog with Cancel
        if (dialog != null)
        {
            dialog.ShowMiddle(
                "Step Verification",
                "Position your camera so that it looks straight at the evidence...",
                "Cancel",
                () => { isCancelled = true; }
            );
        }

        // 2. Countdown 3 seconds
        for (int i = 3; i >= 1; i--)
        {
            if (isCancelled) break;
            await UniTask.Delay(1000);
            if (i == 1 && dialog != null)
            {
                dialog.Close();
            }
        }

        if (isCancelled)
        {
            dialog?.Close();
            return;
        }

        // 3. Take picture from NativeCameraController
        var tcs = new UniTaskCompletionSource<Texture2D>();
        NativeCameraController.TakePicture((success, texture) =>
        {
            if (success && texture != null)
            {
                tcs.TrySetResult(texture);
            }
            else
            {
                tcs.TrySetResult(null);
            }
        });

        var capturedTexture = await tcs.Task;
        if (capturedTexture == null)
        {
            dialog?.ShowMiddle("Step Verification", "Failed to capture image from camera.", "OK", () => { });
            return;
        }

        // Show analyzing message
        dialog?.ShowMiddle("Step Verification", "Analyzing camera capture with AI...", "Cancel", () => { isCancelled = true; });

        byte[] imageBytes = capturedTexture.EncodeToJPG(85);
        Destroy(capturedTexture);

        var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
        if (aiManager == null)
        {
            dialog?.ShowMiddle("Step Verification", "AI Manager is not initialized.", "OK", () => { }, closeActive: true);
            return;
        }

        // 4. Submit setup call
        string taskTitle = string.IsNullOrEmpty(_step.Name) ? "Step Verification" : _step.Name;
        string instructions = $"title: {_step.Name}\ndescription: {_step.Description}";

        var setupResponse = await aiManager.passthroughFrameInterpretationSetup(taskTitle, instructions);
        if (setupResponse == null || string.IsNullOrEmpty(setupResponse.ThreadId) || string.IsNullOrEmpty(setupResponse.TaskId))
        {
            dialog?.ShowMiddle("Step Verification", "Failed to initialize step interpretation setup on the server.", "OK", () => { }, closeActive: true);
            return;
        }

        if (isCancelled)
        {
            dialog?.Close();
            return;
        }

        // 5. Submit track call
        var trackResponse = await aiManager.passthroughFrameInterpretationTrack(
            setupResponse.ThreadId,
            setupResponse.TaskId,
            imageBytes,
            0,
            "gpt-4o",
            setupResponse.AssistantId);

        if (trackResponse == null)
        {
            dialog?.ShowMiddle("Step Verification", "Failed to receive verification response from the server.", "OK", () => { }, closeActive: true);
            return;
        }

        // 6. Process verification result
        if (trackResponse.IsCompleted)
        {
            _completedStepIds.Add(_step.Id.ToString());
            UpdateCompletedVisual(true);

            dialog?.ShowMiddle("Step Completed", trackResponse.Interpretation ?? "Step has been successfully verified as completed!", "OK", () =>
            {
                // Advance to next step and auto-scroll horizontal view upon OK click
                RootObject.Instance.LEE.StepManager.GoToNextStep();
                var nextStep = RootObject.Instance.LEE.StepManager.CurrentStep;
                if (nextStep != null)
                {
                    RootView_v2.Instance?.activityView?.stepsListView?.ScrollToStep(nextStep.Id);
                }
            }, closeActive: true);
        }
        else
        {
            _completedStepIds.Remove(_step.Id.ToString());
            UpdateCompletedVisual(false);

            int totalSubsteps = setupResponse.Substeps != null ? setupResponse.Substeps.Count : 0;
            string nextSubstepText = null;
            if (!string.IsNullOrEmpty(trackResponse.NextSubstep))
            {
                nextSubstepText = trackResponse.NextSubstep;
            }
            else if (setupResponse.Substeps != null && trackResponse.CurrentSubstep < setupResponse.Substeps.Count)
            {
                nextSubstepText = setupResponse.Substeps[trackResponse.CurrentSubstep];
            }

            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(trackResponse.Interpretation))
            {
                sb.AppendLine(trackResponse.Interpretation);
            }
            if (totalSubsteps > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"You are in step {trackResponse.CurrentSubstep} out of {totalSubsteps} steps.");
            }
            if (totalSubsteps > 0 && trackResponse.CurrentSubstep < totalSubsteps && !string.IsNullOrEmpty(nextSubstepText))
            {
                sb.AppendLine($"Next: {nextSubstepText}");
            }

            dialog?.ShowMiddle("Step Verification", sb.ToString(), "OK", () => { }, closeActive: true);
        }
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
