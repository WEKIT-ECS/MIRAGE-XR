using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class BreakingChangeDialog : EditorWindow
    {
        private const string MIGRATION_GUIDE_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/troubleshooting/updating-from-earlier-versions";
        private const string TITLE = "Update Packages";

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private static Action updateClicked;

        public static void ShowDialog(Action onUpdate)
        {
            updateClicked = onUpdate;

            var window = GetWindow<BreakingChangeDialog>();
            window.titleContent = new GUIContent(TITLE);
            window.minSize = new Vector2(500, 140);
            window.maxSize = new Vector2(500, 140);
            window.ShowModalUtility();
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);

            var migrationLink = rootVisualElement.Q<Label>("MigrationLink");
            migrationLink.RegisterCallback<MouseUpEvent>(x =>
            {
                Application.OpenURL(MIGRATION_GUIDE_URL);
            });

            rootVisualElement.Q<Button>("UpdateButton").clicked += () =>
            {
                updateClicked?.Invoke();
                Close();
            };

            rootVisualElement.Q<Button>("CancelButton").clicked += Close;
        }
    }
}
