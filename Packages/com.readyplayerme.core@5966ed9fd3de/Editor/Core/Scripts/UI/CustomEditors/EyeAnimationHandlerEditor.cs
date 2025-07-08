using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    [CustomEditor(typeof(EyeAnimationHandler))]
    public class EyeAnimationHandlerEditor : UnityEditor.Editor
    {
        private const string BLINK_DURATION = "blinkDuration";
        private const string BLINK_INTERVAL = "blinkInterval";
        private readonly GUIContent blinkSpeedLabel =
            new GUIContent("Blink Duration", "Effects the duration of the avatar blink animation in seconds.");

        private readonly GUIContent blinkIntervalLabel =
            new GUIContent("Blink Interval", "Effects the amount of time in between each blink in seconds..");

        private SerializedProperty blinkDuration;
        private SerializedProperty blinkInterval;

        public override void OnInspectorGUI()
        {
            DrawPropertyField(blinkDuration, blinkSpeedLabel);
            DrawPropertyField(blinkInterval, blinkIntervalLabel);
        }

        private void OnEnable()
        {
            blinkDuration = serializedObject.FindProperty(BLINK_DURATION);
            blinkInterval = serializedObject.FindProperty(BLINK_INTERVAL);
        }

        private void DrawPropertyField(SerializedProperty property, GUIContent content)
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, content);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                (target as EyeAnimationHandler)?.Initialize();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
