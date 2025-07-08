using System;
using System.Linq;
using JetBrains.Annotations;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ReadyPlayerMe.Core.Editor
{
    public class AvatarBodyTypeTemplate : VisualElement
    {
        private const string XML_PATH = "AvatarBodyTypeTemplate";
        [CanBeNull] private const string AVATAR_BODY_TYPE_DROPDOWN_FIELD = "DropdownField";
        private const string AVATAR_BODY_TYPE_HELP_BUTTON = "BodyTypeHelpButton";

        public new class UxmlFactory : UxmlFactory<AvatarBodyTypeTemplate, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        public AvatarBodyTypeTemplate()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(XML_PATH);
            visualTree.CloneTree(this);

            var bodyType = CoreSettingsHandler.CoreSettings.BodyType;

            var field = this.Q<DropdownField>(AVATAR_BODY_TYPE_DROPDOWN_FIELD);
            field.choices = Enum.GetNames(typeof(BodyType)).AsEnumerable().Where(bodyType => bodyType != BodyType.None.ToString()).ToList();
            field.value = bodyType.ToString();
            field.RegisterValueChangedCallback(OnBodyTypeChanged);
            this.Q<Button>(AVATAR_BODY_TYPE_HELP_BUTTON).clicked += OnHelpButtonClicked;
        }

        private void OnHelpButtonClicked()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.Avatars);
            Application.OpenURL(Constants.Links.AVATARS);
        }

        private void OnBodyTypeChanged(ChangeEvent<string> evt)
        {
            var newBodyType = Enum.Parse<BodyType>(evt.newValue);
            CoreSettingsSetter.SaveBodyType(newBodyType);
        }
    }
}
