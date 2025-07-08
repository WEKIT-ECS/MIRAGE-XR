using ReadyPlayerMe.Core.Analytics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class AvatarConfigTemplate : VisualElement
    {
        private const string XML_PATH = "AvatarConfigTemplate";
        private const string AVATAR_CONFIG_FIELD = "AvatarConfigField";
        private const string AVATAR_CONFIG_TOOLTIP = "Assign an avatar configuration to include Avatar API request parameters.";
        private const string AVATAR_CONFIG_LABEL = "AvatarConfigLabel";
        private const string AVATAR_CONFIG_HELP_BUTTON = "AvatarConfigHelpButton";

        public new class UxmlFactory : UxmlFactory<AvatarConfigTemplate, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        public AvatarConfigTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            this.Q<Label>(AVATAR_CONFIG_LABEL).tooltip = AVATAR_CONFIG_TOOLTIP;
            this.Q<Button>(AVATAR_CONFIG_HELP_BUTTON).clicked += OnHelpButtonClicked;

            var avatarConfigField = this.Q<ObjectField>(AVATAR_CONFIG_FIELD);
            avatarConfigField.value = AvatarLoaderSettingsHelper.AvatarLoaderSettings.AvatarConfig;
            avatarConfigField.RegisterValueChangedCallback(OnAvatarConfigChanged);
        }

        private void OnHelpButtonClicked()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.AvatarConfig);
            Application.OpenURL(Constants.Links.DOCS_AVATAR_CONFIG_LINK);
        }

        private void OnAvatarConfigChanged(ChangeEvent<Object> evt)
        {
            AvatarLoaderSettingsHelper.SaveAvatarConfig(evt.newValue as AvatarConfig);
        }
    }
}
