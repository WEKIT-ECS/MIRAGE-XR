using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class AvatarUrlTemplate : VisualElement
    {
        private const string XML_PATH = "AvatarUrlTemplate";
        private const string URL_SAVE_KEY = "UrlSaveKey";
        private const string URL_ELEMENT = "UrlField";
        private const string ERROR_LABEL = "ErrorLabel";
        private const string HELP_BUTTON = "HelpButton";
        private const string ERROR_HELP_URL = "https://docs.readyplayer.me/ready-player-me/customizing-guides/avatar-creator/avatar-urls";

        public new class UxmlFactory : UxmlFactory<AvatarUrlTemplate, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private readonly TextField urlField;
        private readonly Label errorLabel;

        public AvatarUrlTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            var url = EditorPrefs.GetString(URL_SAVE_KEY);

            urlField = this.Q<TextField>(URL_ELEMENT);
            urlField.value = url;
            urlField.RegisterValueChangedCallback(OnValueChanged);

            errorLabel = this.Q<Label>(ERROR_LABEL);
            errorLabel.visible = !url.IsUrlShortcodeValid();
            errorLabel.RegisterCallback<MouseDownEvent>(_ =>
            {
                AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.LoadingAvatars);
                Application.OpenURL(ERROR_HELP_URL);
            });

            var helpButton = this.Q<Button>(HELP_BUTTON);
            helpButton.clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.LoadingAvatars);
                Application.OpenURL(Constants.Links.DOCS_AVATAR_LOADER_WINDOW);
            };
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            errorLabel.visible = !(!string.IsNullOrEmpty(evt.newValue) && evt.newValue.IsUrlShortcodeValid());
            EditorPrefs.SetString(URL_SAVE_KEY, evt.newValue);
        }

        public bool TryGetUrl(out string url)
        {
            if (string.IsNullOrEmpty(urlField.text))
            {
                url = string.Empty;
                return false;
            }

            url = urlField.text;
            return true;
        }
    }
}
