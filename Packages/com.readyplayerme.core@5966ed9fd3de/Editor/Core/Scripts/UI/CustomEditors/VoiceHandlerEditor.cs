using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    [CustomEditor(typeof(VoiceHandler))]
    public class VoiceHandlerEditor : UnityEditor.Editor
    {
        private const string AUDIO_CLIP = "AudioClip";
        private const string PROPERTY_PATH = "AudioSource";
        private const string AUDIO_SOURCE = PROPERTY_PATH;
        private const string AUDIO_PROVIDER = "AudioProvider";
        private const string MICROPHONE_IS_NOT_SUPPORTED_IN_WEBGL = "Microphone is not supported in WebGL.";
        private const string TEST_AUDIO_CLIP_PLAY_MODE = "Test Audio Clip [Play Mode]";
        private readonly GUIContent audioClipLabel = new GUIContent(AUDIO_CLIP, "AudioClip to play.");

        private readonly GUIContent audioSourceLabel = new GUIContent(AUDIO_SOURCE,
            "AudioSource that will play the audio. If not assigned spawns on the avatar root object.");

        private readonly GUIContent audioProviderLabel =
            new GUIContent(AUDIO_PROVIDER, "Selection for source of the audio.");

        private SerializedProperty audioClip;
        private SerializedProperty audioSource;
        private SerializedProperty audioProvider;

        public override void OnInspectorGUI()
        {
            DrawPropertyField(audioProvider, audioProviderLabel);
            DrawPropertyField(audioSource, audioSourceLabel);

            if (audioProvider.intValue == (int) AudioProviderType.AudioClip)
            {
                DrawPropertyField(audioClip, audioClipLabel);

                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button(TEST_AUDIO_CLIP_PLAY_MODE))
                {
                    ((VoiceHandler) target).PlayCurrentAudioClip();
                }

                GUI.enabled = true;
            }

#if UNITY_WEBGL
            if (audioProvider.intValue == (int) AudioProviderType.Microphone)
            {
                EditorGUILayout.HelpBox(MICROPHONE_IS_NOT_SUPPORTED_IN_WEBGL, MessageType.Warning);
            }
#endif
        }

        private void OnEnable()
        {
            audioClip = serializedObject.FindProperty(AUDIO_CLIP);
            audioSource = serializedObject.FindProperty(AUDIO_SOURCE);
            audioProvider = serializedObject.FindProperty(AUDIO_PROVIDER);
        }

        private void DrawPropertyField(SerializedProperty property, GUIContent content)
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, content);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ((VoiceHandler) target).InitializeAudio();
            }
        }
    }
}
