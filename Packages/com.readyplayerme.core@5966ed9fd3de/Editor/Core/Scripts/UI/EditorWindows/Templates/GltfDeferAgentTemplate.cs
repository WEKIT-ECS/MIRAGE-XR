using ReadyPlayerMe.Core.Analytics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class GltfDeferAgentTemplate : VisualElement
    {
        private const string XML_PATH = "GltfDeferAgentTemplate";
        private const string DEFER_AGENT_FIELD = "DeferAgentField";
        private const string DEFER_AGENT_TOOLTIP = "Assign a defer agent which decides how the glTF will be loaded.";
        private const string DEFER_AGENT_LABEL = "DeferAgentLabel";
        private const string DEFER_AGENT_HELP_BUTTON = "DeferAgentHelpButton";

        public new class UxmlFactory : UxmlFactory<GltfDeferAgentTemplate, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        public GltfDeferAgentTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            this.Q<Label>(DEFER_AGENT_LABEL).tooltip = DEFER_AGENT_TOOLTIP;
            this.Q<Button>(DEFER_AGENT_HELP_BUTTON).clicked += OnHelpButtonClicked;

            var deferAgentField = this.Q<ObjectField>(DEFER_AGENT_FIELD);
            deferAgentField.value = AvatarLoaderSettingsHelper.AvatarLoaderSettings.GLTFDeferAgent;
            deferAgentField.RegisterValueChangedCallback(OnAvatarConfigChanged);
        }

        private void OnHelpButtonClicked()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.GltfDeferAgent);
            Application.OpenURL(Constants.Links.DOCS_DEFER_AGENT_LINK);
        }

        private void OnAvatarConfigChanged(ChangeEvent<Object> evt)
        {
            AvatarLoaderSettingsHelper.SaveDeferAgent(evt.newValue as GLTFDeferAgent);
        }
    }
}
