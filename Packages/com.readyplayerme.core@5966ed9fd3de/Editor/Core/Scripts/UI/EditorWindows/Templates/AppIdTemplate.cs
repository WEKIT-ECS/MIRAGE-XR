using System;
using ReadyPlayerMe.Core.Analytics;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class AppIdTemplate : VisualElement
    {
        private const string XML_PATH = "AppIdTemplate";
        private const string APPID_VALIDATION_ERROR = "Please enter a valid app id. Read more about this issue.";

        public new class UxmlFactory : UxmlFactory<AppIdTemplate, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private readonly TextField appIdField;
        private readonly string appId;

        public event Action<string> OnAppIdChanged;
        
        public AppIdTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            appId = CoreSettingsHandler.CoreSettings.AppId;
            appIdField = this.Q<TextField>("AppIdField");
            appIdField.value = appId;

            this.Q<Button>("AppIdHelpButton").clicked += OnAppIdHelpClicked;

            var errorIcon = this.Q<VisualElement>("ErrorIcon");
            errorIcon.tooltip = APPID_VALIDATION_ERROR;
            appIdField.RegisterCallback<FocusOutEvent>(OnAppIdFocusOut);
            appIdField.RegisterValueChangedCallback(evt => OnAppIdValueChanged(evt, errorIcon));
        }

        private static void OnAppIdHelpClicked()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.Subdomain);
            Application.OpenURL(Constants.Links.APP_ID);
        }

        private void OnAppIdFocusOut(FocusOutEvent _)
        {
            if (IsValidAppId())
            {
                SaveAppId();
            }
        }

        private void OnAppIdValueChanged(ChangeEvent<string> evt, VisualElement errorIcon)
        {
            errorIcon.visible = !IsValidAppId();
            OnAppIdChanged?.Invoke(evt.newValue);
        }

        private bool IsValidAppId()
        {
            return !string.IsNullOrEmpty(appIdField.value);
        }

        private void SaveAppId()
        {
            var id = CoreSettingsHandler.CoreSettings.AppId;
            if (id != appId)
            {
                AnalyticsEditorLogger.EventLogger.LogUpdatePartnerURL(id, appIdField.value);
            }

            CoreSettingsSetter.SaveAppId(appIdField.value);
        }
    }
}
