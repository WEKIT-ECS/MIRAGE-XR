using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text textStepName;
        [SerializeField] private TMP_Text textStepDescription;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonMenu;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private Toggle _stepCompletedToggle;
        [SerializeField] private GameObject _stepSelected;
        [SerializeField] private GameObject deleteConfirmation;
        [SerializeField] private Button buttonDeleteConfirmation;
        [SerializeField] private Button buttonDeleteCancel;
        [SerializeField] private Image backgroundImage;

        private ActivityStep _step;
        private UnityAction<ActivityStep> _onClick;
        private UnityAction<ActivityStep> _onMenuClick;
        private ButtonLongPress _deleteButtonLongPress;
        private Color _defaultBackgroundColor;

        public void Initialize(ActivityStep step, UnityAction<ActivityStep> onClick, UnityAction<ActivityStep> onMenuClick)
        {
            _step = step;
            _onClick = onClick;
            _onMenuClick = onMenuClick;
            var number = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id);
            textStepName.text = $"{number} {step.Name}";
            var data = HyperlinkPositionData.SplitPositionsFromText(_step.Description);
            textStepDescription.text = data.DisplayText;
            button.onClick.AddListener(OnButtonClick);
            buttonMenu.onClick.AddListener(OnButtonMenuClick);
            buttonDelete.onClick.AddListener(OnButtonDeleteClick);
            buttonDeleteConfirmation.onClick.AddListener(DeleteStep);
            buttonDeleteCancel.onClick.AddListener(HideDeleteConfirmation);
            _stepCompletedToggle.onValueChanged.AddListener(OnStepCompleted);
            _defaultBackgroundColor = backgroundImage.color;
            HideDeleteConfirmation();
            InitializeDeleteLongPress();
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            OnEditorModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClick);
            buttonMenu.onClick.RemoveListener(OnButtonMenuClick);
            buttonDelete.onClick.RemoveListener(OnButtonDeleteClick);
            buttonDeleteConfirmation.onClick.RemoveListener(DeleteStep);
            buttonDeleteCancel.onClick.RemoveListener(HideDeleteConfirmation);
            _stepCompletedToggle.onValueChanged.RemoveListener(OnStepCompleted);
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
            if (_deleteButtonLongPress == null)
            {
                return;
            }

            _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.gameObject.SetActive(!value);
            buttonMenu.gameObject.SetActive(value);
            if (!value)
            {
                HideDeleteConfirmation();
            }
        }

        private void OnStepCompleted(bool isToggled)
        {
            if (isToggled)
            {
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                CheckStepCompletedAsync().Forget();
            }
        }

        private async UniTask CheckStepCompletedAsync()
        {
            var dialog = MenuManager.Instance?.Dialog;
            bool isCancelled = false;

            // 1. Show instruction dialogue
            dialog?.ShowMiddle(
                "Step Verification",
                "Position your head so that the camera looks straight at the evidence...",
                "Cancel",
                () => { isCancelled = true; }
            );

            // 2. Create external floating countdown at 60cm distance in front of the user
            var countdownObj = CreateFloatingCountdown(out var countdownText);

            for (int i = 5; i >= 1; i--)
            {
                if (isCancelled)
                {
                    break;
                }

                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                }
                UpdateFloatingCountdownTransform(countdownObj);

                await UniTask.Delay(1000);

                if (i == 1)
                {
                    // Close the instruction dialogue when it has counted down to 1 and before the picture is taken
                    dialog?.Close();
                }
            }

            if (countdownObj != null)
            {
                Destroy(countdownObj);
            }

            if (isCancelled)
            {
                dialog?.Close();
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                return;
            }

            // 3. Take picture from camera
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
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                return;
            }

            byte[] imageBytes = capturedTexture.EncodeToJPG(85);
            Destroy(capturedTexture);

            var aiManager = RootObject.Instance.LEE.ArtificialIntelligenceManager;
            if (aiManager == null)
            {
                dialog?.ShowMiddle("Step Verification", "AI Manager is not initialized.", "OK", () => { });
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                return;
            }

            // 4. Submit setup call
            string taskTitle = string.IsNullOrEmpty(_step.Name) ? "Step Verification" : _step.Name;
            string instructions = $"title: {_step.Name}\ndescription: {_step.Description}";

            var setupResponse = await aiManager.passthroughFrameInterpretationSetup(taskTitle, instructions);
            if (setupResponse == null || string.IsNullOrEmpty(setupResponse.ThreadId) || string.IsNullOrEmpty(setupResponse.TaskId))
            {
                dialog?.ShowMiddle("Step Verification", "Failed to initialize step interpretation setup on the server.", "OK", () => { });
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                return;
            }

            // 5. Show thinking animation while waiting for track response
            var thinkingIndicator = await SpawnThinkingIndicatorAsync();

            var trackResponse = await aiManager.passthroughFrameInterpretationTrack(
                setupResponse.ThreadId,
                setupResponse.TaskId,
                imageBytes,
                0,
                "gpt-4o",
                setupResponse.AssistantId);

            if (thinkingIndicator != null)
            {
                Destroy(thinkingIndicator);
            }

            if (trackResponse == null)
            {
                dialog?.ShowMiddle("Step Verification", "Failed to receive verification response from the server.", "OK", () => { });
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                return;
            }

            // 6. Process verification result
            if (trackResponse.IsCompleted)
            {
                _stepCompletedToggle.SetIsOnWithoutNotify(true);
                dialog?.ShowMiddle("Step Completed", trackResponse.Interpretation ?? "Step has been successfully verified as completed!", "OK", () => { });
            }
            else
            {
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
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
                    sb.AppendLine();
                    sb.AppendLine($"Next step to do:\n{nextSubstepText}");
                }

                dialog?.ShowMiddle("Step Incomplete", sb.ToString().TrimEnd(), "OK", () => { });
            }
        }

        private GameObject CreateFloatingCountdown(out TMP_Text tmpText)
        {
            var canvasObj = new GameObject("StepVerificationFloatingCountdown");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10;

            var rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 250);
            rectTransform.localScale = Vector3.one * 0.001f;

            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var textObj = new GameObject("CountdownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(canvasObj.transform, false);
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 140;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.2f, 0.75f, 1f, 1f);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            tmpText = tmp;
            UpdateFloatingCountdownTransform(canvasObj);
            return canvasObj;
        }

        private void UpdateFloatingCountdownTransform(GameObject countdownObj)
        {
            if (countdownObj == null) return;
            var cam = Camera.main;
            if (cam != null)
            {
                countdownObj.transform.position = cam.transform.position + cam.transform.forward * 0.6f;
                countdownObj.transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
            }
        }

        private async UniTask<GameObject> SpawnThinkingIndicatorAsync()
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>("LoadingIndicator");
                var prefab = await handle.Task;
                if (prefab != null)
                {
                    var cam = Camera.main;
                    Vector3 spawnPos = cam != null ? cam.transform.position + cam.transform.forward * 0.6f : Vector3.forward * 0.6f;
                    Quaternion spawnRot = cam != null ? Quaternion.LookRotation(cam.transform.forward, cam.transform.up) : Quaternion.identity;
                    var indicator = Instantiate(prefab, spawnPos, spawnRot);
                    indicator.SetActive(true);
                    return indicator;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[StepItemView] Could not instantiate LoadingIndicator via Addressables: " + e.Message);
            }
            return null;
        }

        private void OnButtonClick()
        {
            _onClick?.Invoke(_step);
        }

        private void OnButtonMenuClick()
        {
            //TODO
        }

        private void OnButtonDeleteClick()
        {
            _deleteButtonLongPress?.ConsumeLongPress();
        }

        private void InitializeDeleteLongPress()
        {
            _deleteButtonLongPress = buttonDelete.GetComponent<ButtonLongPress>();
            _deleteButtonLongPress.onHoldProgressChanged.AddListener(OnDeleteHoldProgressChanged);
        }

        private void OnDeleteHoldProgressChanged(float progress)
        {
            if (deleteConfirmation != null && deleteConfirmation.activeSelf)
            {
                backgroundImage.color = GetDeleteHoldColor();
                return;
            }

            if (progress >= 1f)
            {
                ShowDeleteConfirmation();
                return;
            }

            backgroundImage.color = Color.Lerp(_defaultBackgroundColor, GetDeleteHoldColor(), progress);
        }

        private void ShowDeleteConfirmation()
        {
            if (deleteConfirmation != null)
            {
                deleteConfirmation.SetActive(true);
            }

            backgroundImage.color = GetDeleteHoldColor();
        }

        private void HideDeleteConfirmation()
        {
            if (deleteConfirmation != null)
            {
                deleteConfirmation.SetActive(false);
            }

            backgroundImage.color = _defaultBackgroundColor;
        }

        private Color GetDeleteHoldColor()
        {
            var color = _deleteButtonLongPress.holdColor;
            color.a = _defaultBackgroundColor.a;
            return color;
        }

        public void OnStepSelected(bool value)
        {
            _stepSelected.SetActive(value);
        }

        private void DeleteStep()
        {
            _onMenuClick?.Invoke(_step);
        }
    }
}
