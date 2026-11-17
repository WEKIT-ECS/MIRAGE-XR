using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MirageXR
{
    public static class StepVerificationController
    {
        private static readonly HashSet<string> _completedStepIds = new();

        public static bool IsStepCompleted(string stepId)
        {
            return !string.IsNullOrEmpty(stepId) && _completedStepIds.Contains(stepId);
        }

        public static void SetStepCompleted(string stepId, bool completed)
        {
            if (string.IsNullOrEmpty(stepId)) return;
            if (completed)
            {
                _completedStepIds.Add(stepId);
            }
            else
            {
                _completedStepIds.Remove(stepId);
            }
        }

        public static void ClearCompletedSteps()
        {
            _completedStepIds.Clear();
        }

        public static async UniTask<bool> CheckStepCompletedAsync(
            ActivityStep step,
            bool isSpatial = false,
            Action<bool> onResult = null,
            Action onAdvanceStep = null)
        {
            if (step == null) return false;

            var dialog = RootView_v2.Instance?.dialog ?? MenuManager.Instance?.Dialog;
            bool isCancelled = false;

            // 1. Show instruction dialog with Cancel
            string instructionMessage = isSpatial
                ? "Position your head so that the camera looks straight at the evidence..."
                : "Position your camera so that it looks straight at the evidence...";

            if (dialog != null)
            {
                dialog.ShowMiddle(
                    "Step Verification",
                    instructionMessage,
                    "Cancel",
                    () => { isCancelled = true; },
                    closeActive: true
                );
            }

            // 2. Countdown: spatial floating countdown (5s) or screen dialog countdown (3s)
            GameObject countdownObj = null;
            TMP_Text countdownText = null;

            if (isSpatial)
            {
                countdownObj = CreateFloatingCountdown(out countdownText);
            }

            int countdownSeconds = isSpatial ? 5 : 3;
            for (int i = countdownSeconds; i >= 1; i--)
            {
                if (isCancelled) break;

                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                }
                if (isSpatial && countdownObj != null)
                {
                    UpdateFloatingCountdownTransform(countdownObj);
                }

                await UniTask.Delay(1000);

                if (i == 1 && dialog != null)
                {
                    dialog.Close();
                }
            }

            if (countdownObj != null)
            {
                UnityEngine.Object.Destroy(countdownObj);
            }

            if (isCancelled)
            {
                dialog?.Close();
                onResult?.Invoke(false);
                return false;
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
                dialog?.ShowMiddle("Step Verification", "Failed to capture image from camera.", "OK", () => { }, closeActive: true);
                onResult?.Invoke(false);
                return false;
            }

            // Show thinking animation while waiting for track response
            var thinkingIndicator = await SpawnThinkingIndicatorAsync();

            byte[] imageBytes = capturedTexture.EncodeToJPG(85);
            UnityEngine.Object.Destroy(capturedTexture);

            var aiManager = RootObject.Instance?.LEE?.ArtificialIntelligenceManager;
            if (aiManager == null)
            {
                if (thinkingIndicator != null) UnityEngine.Object.Destroy(thinkingIndicator);
                dialog?.ShowMiddle("Step Verification", "AI Manager is not initialized.", "OK", () => { }, closeActive: true);
                onResult?.Invoke(false);
                return false;
            }

            // 4. Submit setup call
            string taskTitle = string.IsNullOrEmpty(step.Name) ? "Step Verification" : step.Name;
            string instructions = $"title: {step.Name}\ndescription: {step.Description}";
            string activityId = RootObject.Instance?.LEE?.ActivityManager?.ActivityId.ToString() ?? "";
            string stepId = step.Id.ToString();
            string currentUserName = RootObject.Instance?.LEE?.AuthorizationManager?.UserName;
            string userId = !string.IsNullOrEmpty(currentUserName) ? currentUserName : "anonymous";

            var setupResponse = await aiManager.PassthroughFrameInterpretationSetupAsync(
                taskTitle,
                instructions,
                model: "gpt-4o",
                userId: userId,
                activityId: activityId,
                stepId: stepId);

            if (setupResponse == null || string.IsNullOrEmpty(setupResponse.ThreadId) || string.IsNullOrEmpty(setupResponse.TaskId))
            {
                if (thinkingIndicator != null) UnityEngine.Object.Destroy(thinkingIndicator);
                dialog?.ShowMiddle("Step Verification", "Failed to initialize step interpretation setup on the server.", "OK", () => { }, closeActive: true);
                onResult?.Invoke(false);
                return false;
            }

            var trackResponse = await aiManager.PassthroughFrameInterpretationTrackAsync(
                setupResponse.ThreadId,
                setupResponse.TaskId,
                imageBytes,
                currentStep: 0,
                model: "gpt-4o",
                assistantId: setupResponse.AssistantId,
                activityId: activityId,
                stepId: stepId);

            if (thinkingIndicator != null)
            {
                UnityEngine.Object.Destroy(thinkingIndicator);
            }

            if (trackResponse == null)
            {
                dialog?.ShowMiddle("Step Verification", "Failed to receive verification response from the server.", "OK", () => { }, closeActive: true);
                onResult?.Invoke(false);
                return false;
            }

            // 6. Process verification result
            if (trackResponse.IsCompleted)
            {
                SetStepCompleted(step.Id.ToString(), true);
                onResult?.Invoke(true);

                dialog?.ShowMiddle("Step Completed", trackResponse.Interpretation ?? "Step has been successfully verified as completed!", "OK", () =>
                {
                    onAdvanceStep?.Invoke();
                }, closeActive: true);

                return true;
            }
            else
            {
                SetStepCompleted(step.Id.ToString(), false);
                onResult?.Invoke(false);

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

                var sb = new StringBuilder();
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

                dialog?.ShowMiddle("Step Incomplete", sb.ToString().TrimEnd(), "OK", () => { }, closeActive: true);
                return false;
            }
        }

        private static GameObject CreateFloatingCountdown(out TMP_Text tmpText)
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

        private static void UpdateFloatingCountdownTransform(GameObject countdownObj)
        {
            if (countdownObj == null) return;
            var cam = Camera.main;
            if (cam != null)
            {
                countdownObj.transform.position = cam.transform.position + cam.transform.forward * 0.6f;
                countdownObj.transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
            }
        }

        private static async UniTask<GameObject> SpawnThinkingIndicatorAsync()
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
                    var indicator = UnityEngine.Object.Instantiate(prefab, spawnPos, spawnRot);
                    indicator.SetActive(true);
                    return indicator;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[StepVerificationController] Could not instantiate LoadingIndicator via Addressables: " + e.Message);
            }
            return null;
        }
    }
}
