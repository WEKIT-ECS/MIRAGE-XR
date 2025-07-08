using System;
using System.Linq;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class SubdomainTemplate : VisualElement
    {
        private const string XML_PATH = "SubdomainTemplate";
        private const string DOMAIN_VALIDATION_ERROR = "Please enter a valid partner subdomain (e.g. demo). Read more about this issue.";
        private const string WEB_VIEW_PARTNER_SAVE_KEY = "WebViewPartnerSubdomainName";

        public new class UxmlFactory : UxmlFactory<SubdomainTemplate, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private readonly TextField subdomainField;

        public event Action<string> OnSubdomainChanged;

        private string partnerSubdomain;
        private bool isSubdomainFieldEnabled;

        public SubdomainTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            partnerSubdomain = CoreSettingsHandler.CoreSettings.Subdomain;
            subdomainField = this.Q<TextField>("SubdomainField");
            subdomainField.value = partnerSubdomain;
            var errorIcon = this.Q<VisualElement>("ErrorIcon");
            errorIcon.tooltip = DOMAIN_VALIDATION_ERROR;

            this.Q<Button>("SubdomainHelpButton").clicked += OnSubdomainHelpClicked;

            subdomainField.RegisterValueChangedCallback(SubdomainChanged(errorIcon));
            subdomainField.RegisterCallback<FocusOutEvent>(OnSubdomainFocusOut);
        }

        private static void OnSubdomainHelpClicked()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.Subdomain);
            Application.OpenURL(Constants.Links.DOCS_PARTNERS_LINK);
        }

        private void OnSubdomainFocusOut(FocusOutEvent _)
        {
            if (IsValidSubdomain())
            {
                SaveSubdomain();
            }
        }

        private EventCallback<ChangeEvent<string>> SubdomainChanged(VisualElement errorIcon)
        {
            return changeEvent =>
            {
                partnerSubdomain = ExtractSubdomain(changeEvent.newValue);
                errorIcon.visible = !IsValidSubdomain();
                if (changeEvent.previousValue == partnerSubdomain && !changeEvent.newValue.Contains(".")) return;
                subdomainField.SetValueWithoutNotify(partnerSubdomain);
                if (changeEvent.newValue != partnerSubdomain)
                {
                    subdomainField.Focus(); // forces text to resize, making new subdomain value visible
                }
                OnSubdomainChanged?.Invoke(partnerSubdomain);
            };
        }

        private static string ExtractSubdomain(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var host = uri.Host;
                var indexOfColon = host.IndexOf(':');
                if (indexOfColon >= 0)
                {
                    host = host.Substring(0, indexOfColon);
                }

                var hostParts = host.Split('.');
                if (hostParts.Length > 0)
                {
                    return hostParts[0];
                }
            }

            return url;
        }

        private bool IsValidSubdomain()
        {
            return !partnerSubdomain.All(char.IsWhiteSpace) && !partnerSubdomain.Contains('/') && !partnerSubdomain.IsUrlShortcodeValid();
        }

        private void SaveSubdomain()
        {
            EditorPrefs.SetString(WEB_VIEW_PARTNER_SAVE_KEY, partnerSubdomain);
            var subDomain = CoreSettingsHandler.CoreSettings.Subdomain;
            if (subDomain == partnerSubdomain || !IsValidSubdomain()) return;
            AnalyticsEditorLogger.EventLogger.LogUpdatePartnerURL(subDomain, partnerSubdomain);
            CoreSettingsSetter.SaveSubDomain(partnerSubdomain);
        }
    }
}
